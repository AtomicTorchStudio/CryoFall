namespace AtomicTorch.CBND.CoreMod.Systems.StructureDecaySystem
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.ServicesServer;

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
                callback: ServerTimerTickCallback,
                name: "System." + this.ShortId);
        }

        private static void ServerRefreshLandClaimAreasGroup(ILogicObject areasGroup)
        {
            if (areasGroup.IsDestroyed)
            {
                return;
            }

            var areas = LandClaimAreasGroup.GetPrivateState(areasGroup).ServerLandClaimsAreas;
            foreach (var area in areas)
            {
                if (area.IsDestroyed)
                {
                    continue;
                }

                var areaBounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area, addGracePadding: true);
                var owners = LandClaimArea.GetPrivateState(area).LandOwners;

                foreach (var owner in owners)
                {
                    var character = Server.Characters.GetPlayerCharacter(owner);
                    if (character == null
                        || !character.ServerIsOnline)
                    {
                        continue;
                    }

                    if (!areaBounds.Contains(character.TilePosition))
                    {
                        continue;
                    }

                    // the land claim area contains an online owner character
                    ServerResetDecayTimer();
                    return;
                }
            }

            // helper method to reset the decay timer for all land claim buildings inside this areas group
            void ServerResetDecayTimer()
            {
                var decayDelayDuration = LandClaimSystem.ServerGetDecayDelayDurationForLandClaimAreas(areas);

                foreach (var area in areas)
                {
                    var worldObject = LandClaimArea.GetPrivateState(area)
                                                   .ServerLandClaimWorldObject;

                    StructureDecaySystem.ServerResetDecayTimer(
                        worldObject.GetPrivateState<StructurePrivateState>(),
                        decayDelayDuration);
                }
            }
        }

        // try reset land claim decay timer
        private static async void ServerTimerTickCallback()
        {
            if (isUpdatingNow)
            {
                Logger.Warning(
                    "Cannot process land claim reset decay system tick - not finished the previous update yet");
                return;
            }

            if (!StructureConstants.IsStructuresDecayEnabled)
            {
                return;
            }

            // We will time-slice this update just in case there are too many areas.
            isUpdatingNow = true;
            TempList.AddRange(Server.World.GetGameObjectsOfProto<ILogicObject, LandClaimAreasGroup>());
            await ServerCore.AwaitEndOfFrame;

            try
            {
                foreach (var areasGroup in TempList)
                {
                    ServerRefreshLandClaimAreasGroup(areasGroup);
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