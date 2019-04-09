namespace AtomicTorch.CBND.CoreMod.Systems.StructureDecaySystem
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// The game server should check/poll whether there are any land claim owners inside the land claim area
    /// and reset the decay timer if so.
    public sealed class StructureLandClaimDecayResetSystem : ProtoSystem<StructureLandClaimDecayResetSystem>
    {
        private static readonly ICoreServerService ServerCore
            = IsServer ? Server.Core : null;

        private static readonly List<ILogicObject> TempList
            = new List<ILogicObject>(capacity: 1000);

        private static bool isUpdatingNow;

        public override string Name => "Structure land claim reset system";

        public static void ServerRefreshLandClaimObject(IStaticWorldObject worldObject)
        {
            if (worldObject.IsDestroyed)
            {
                return;
            }

            if (!(worldObject.ProtoStaticWorldObject is IProtoObjectLandClaim))
            {
                // not a land claim structure
                return;
            }

            var area = LandClaimSystem.ServerGetLandClaimArea(worldObject);
            if (area == null)
            {
                // incorrect land claim - no area attached
                return;
            }

            var areaBounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area);
            var owners = LandClaimArea.GetPrivateState(area).LandOwners;

            foreach (var owner in owners)
            {
                var character = Server.Characters.GetPlayerCharacter(owner);
                if (character == null
                    || !character.IsOnline)
                {
                    continue;
                }

                if (!areaBounds.Contains(character.TilePosition))
                {
                    continue;
                }

                // the land claim contains an online owner character
                // reset the decay timer for this land claim
                StructureDecaySystem.ServerResetDecayTimer(
                    worldObject.GetPrivateState<StructurePrivateState>());

                using (var tempVisitedAreas = Api.Shared.WrapObjectInTempList(area))
                {
                    ServerResetDecayTimerRecursively(tempVisitedAreas.AsList(),
                                                     areaBounds,
                                                     character);
                }

                return;
            }
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                // only server will refresh
                return;
            }

            // configure time interval trigger
            TriggerTimeInterval.ServerConfigureAndRegister(
                interval: TimeSpan.FromSeconds(StructureConstants
                                                   .StructureDecayLandClaimResetSystemUpdateIntervalSeconds),
                callback: this.ServerTimerTickCallback,
                name: "System." + this.ShortId);
        }

        // Iterate over all not-visited neighbor areas of this owner and reset their decay timers, recursively.
        private static void ServerResetDecayTimerRecursively(
            List<ILogicObject> visitedAreas,
            RectangleInt currentAreaBounds,
            ICharacter character)
        {
            using (var tempList = Api.Shared.GetTempList<ILogicObject>())
            {
                LandClaimSystem.SharedGetAreasInBounds(currentAreaBounds.Inflate(1, 1),
                                                       tempList);

                foreach (var otherArea in tempList)
                {
                    if (visitedAreas.Contains(otherArea))
                    {
                        continue;
                    }

                    visitedAreas.Add(otherArea);
                    if (!LandClaimSystem.ServerIsOwnedArea(otherArea, character))
                    {
                        continue;
                    }

                    var worldObject = otherArea.GetPrivateState<LandClaimAreaPrivateState>().ServerLandClaimWorldObject;
                    StructureDecaySystem.ServerResetDecayTimer(
                        worldObject.GetPrivateState<StructurePrivateState>());

                    var otherAreaBounds = LandClaimSystem.SharedGetLandClaimAreaBounds(otherArea);
                    ServerResetDecayTimerRecursively(visitedAreas, otherAreaBounds, character);
                }
            }
        }

        // refresh structures decay
        private async void ServerTimerTickCallback()
        {
            if (isUpdatingNow)
            {
                Logger.Warning(
                    "Cannot process land claim reset decay system tick - not finished the previous update yet");
                return;
            }

            if (!StructureConstants.IsStructureDecayEnabledInEditor
                && Api.IsEditor)
            {
                return;
            }

            // We will time-slice this update just in case there are too many areas.
            isUpdatingNow = true;

            try
            {
                TempList.AddRange(LandClaimSystem.ServerEnumerateAllAreas());

                foreach (var area in TempList)
                {
                    var worldObject = LandClaimArea.GetPrivateState(area)
                                                   .ServerLandClaimWorldObject;
                    ServerRefreshLandClaimObject(worldObject);
                    await ServerCore.YieldIfOutOfTime();
                }
            }
            finally
            {
                isUpdatingNow = false;
                TempList.Clear();
            }
        }
    }
}