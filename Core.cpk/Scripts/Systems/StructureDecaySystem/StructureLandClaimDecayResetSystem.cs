namespace AtomicTorch.CBND.CoreMod.Systems.StructureDecaySystem
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    /// The game server should check/poll whether there are any land claim owners inside the land claim area
    /// and reset the decay timer if so.
    public sealed class StructureLandClaimDecayResetSystem : ProtoSystem<StructureLandClaimDecayResetSystem>
    {
        private static readonly ICharactersServerService ServerCharacters
            = IsServer ? Server.Characters : null;

        public override string Name => "Structure land claim reset system";

        protected override void PrepareSystem()
        {
            if (IsClient
                || !RateStructuresDecayEnabled.SharedValue)
            {
                return;
            }

            // configure time interval trigger
            TriggerEveryFrame.ServerRegister(
                callback: ServerUpdate,
                name: "System." + this.ShortId);
        }

        private static bool ServerGetIsFounderDemoPlayer(List<ILogicObject> areas)
        {
            var isFounderDemoPlayer = false;

            foreach (var area in areas)
            {
                var areaPrivateState = LandClaimArea.GetPrivateState(area);
                var landClaimFounder = areaPrivateState.LandClaimFounder;

                if (string.IsNullOrEmpty(landClaimFounder))
                {
                    // there is no founder (transferred to the faction)
                    return false;
                }

                var founder = ServerCharacters.GetPlayerCharacter(landClaimFounder);

                if (founder.ServerIsDemoVersion)
                {
                    isFounderDemoPlayer = true;
                }
                else
                {
                    // one of the areas' founder is not a demo player
                    return false;
                }
            }

            return isFounderDemoPlayer;
        }

        private static void ServerResetDecayTimerForLandClaimAreasGroup(ILogicObject areasGroup)
        {
            var areasGroupPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            var areasGroupPublicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            var areas = areasGroupPrivateState.ServerLandClaimsAreas;

            // TODO: it's better to move this code to another place as this property is used in several other places
            areasGroupPublicState.IsFounderDemoPlayer = ServerGetIsFounderDemoPlayer(areas);

            // reset the decay timer for all land claim buildings inside this areas group
            var decayDelayDuration = LandClaimSystem.ServerGetDecayDelayDurationForLandClaimAreas(
                areas,
                areasGroupPublicState.IsFounderDemoPlayer,
                out _);

            foreach (var area in areas)
            {
                var worldObject = LandClaimArea.GetPrivateState(area)
                                               .ServerLandClaimWorldObject;

                StructureDecaySystem.ServerResetDecayTimer(
                    worldObject.GetPrivateState<StructurePrivateState>(),
                    decayDelayDuration);
            }
        }

        /// <summary>
        /// Enumerate online players and reset the decay timer for
        /// the owned land claims areas when they're inside them.
        /// </summary>
        private static void ServerUpdate()
        {
            // Check interval (in seconds) for land claims.
            // Currently every 3 seconds every player is checked whether it's inside an owned land claim,
            // and if so, decay for it is reset.
            const double spreadDeltaTime = 3;

            using var tempListPlayers = Api.Shared.GetTempList<ICharacter>();
            PlayerCharacter.Instance
                           .EnumerateGameObjectsWithSpread(tempListPlayers.AsList(),
                                                           spreadDeltaTime: spreadDeltaTime,
                                                           Server.Game.FrameNumber,
                                                           Server.Game.FrameRate);

            foreach (var character in tempListPlayers.AsList())
            {
                if (!character.ServerIsOnline)
                {
                    continue;
                }

                if (!LandClaimSystem.SharedIsOwnedLand(character.TilePosition,
                                                       character,
                                                       requireFactionPermission: false,
                                                       out var ownedArea))
                {
                    // the player is not inside an owned land claim area
                    continue;
                }

                // the land claim area contains an online owner
                var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(ownedArea);
                ServerResetDecayTimerForLandClaimAreasGroup(areasGroup);
            }
        }
    }
}