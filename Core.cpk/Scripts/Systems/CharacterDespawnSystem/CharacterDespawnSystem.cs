namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// Player characters should be despawned on PvE servers after some delay.
    /// They could respawn after that.
    /// </summary>
    public class CharacterDespawnSystem : ProtoSystem<CharacterDespawnSystem>
    {
        public const double OfflineDurationToDespawnWhenInFreeLand = 60 * 60; // 1 hour

        public const double OfflineDurationToDespawnWhenInsideNotOwnedClaim = 10 * 60; // 10 minutes

        public static bool ClientIsDespawned => ClientCurrentCharacterHelper.PrivateState.IsDespawned;

        [NotLocalizable]
        public override string Name => "Character despawn system";

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            if (!PveSystem.ServerIsPvE)
            {
                return;
            }

            if (SharedLocalServerHelper.IsLocalServer)
            {
                // don't despawn offline players on the local server
                return;
            }

            // setup timer (tick every frame)
            TriggerEveryFrame.ServerRegister(
                callback: ServerGlobalUpdate,
                name: "System." + this.ShortId);
        }

        private static void ServerGlobalUpdate()
        {
            var serverTime = Server.Game.FrameTime;
            // perform update once per 10 seconds per player
            const double spreadDeltaTime = 10;

            using var tempListPlayers = Api.Shared.GetTempList<ICharacter>();
            PlayerCharacter.Instance
                           .EnumerateGameObjectsWithSpread(tempListPlayers.AsList(),
                                                           spreadDeltaTime: spreadDeltaTime,
                                                           Server.Game.FrameNumber,
                                                           Server.Game.FrameRate);

            foreach (var character in tempListPlayers.AsList())
            {
                if (ServerIsNeedDespawn(character, serverTime))
                {
                    ServerCharacterDeathMechanic.DespawnCharacter(character);
                }
            }
        }

        private static bool ServerIsCharacterInsideOwnedBase(ICharacter character, out bool isInsideNotOwnedBase)
        {
            var currentBase = LandClaimSystem.SharedGetLandClaimAreasGroup(character.TilePosition);
            if (currentBase is null)
            {
                isInsideNotOwnedBase = false;
                return false;
            }

            foreach (var area in LandClaimAreasGroup.GetPrivateState(currentBase)
                                                    .ServerLandClaimsAreas)
            {
                if (LandClaimSystem.SharedIsOwnedArea(area, character, requireFactionPermission: false))
                {
                    isInsideNotOwnedBase = false;
                    return true;
                }
            }

            isInsideNotOwnedBase = true;
            return false;
        }

        private static bool ServerIsNeedDespawn(ICharacter character, double serverTime)
        {
            var privateState = character.GetPrivateState<PlayerCharacterPrivateState>();
            if (character.ServerIsOnline)
            {
                return false;
            }

            // player offline
            var publicState = character.GetPublicState<PlayerCharacterPublicState>();
            if (publicState.IsDead
                || privateState.IsDespawned)
            {
                return false;
            }

            if (character.ProtoCharacter is PlayerCharacterSpectator
                || Server.Characters.IsSpectator(character))
            {
                return false;
            }

            if (ServerIsCharacterInsideOwnedBase(character, out var isInsideNotOwnedBase))
            {
                return false;
            }

            var durationToDespawn = isInsideNotOwnedBase
                                        ? OfflineDurationToDespawnWhenInsideNotOwnedClaim
                                        : OfflineDurationToDespawnWhenInFreeLand;

            if (serverTime < privateState.ServerLastActiveTime + durationToDespawn)
            {
                // despawn timeout is not reached yet
                return false;
            }

            return true;
        }
    }
}