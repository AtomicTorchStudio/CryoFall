namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    /// <summary>
    /// Player characters should be despawned on PvE servers after some delay.
    /// They could respawn after that.
    /// </summary>
    public class CharacterDespawnSystem : ProtoSystem<CharacterDespawnSystem>
    {
        public const double OfflineDurationToDespawn = 4 * 60 * 60; // 4 hours

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

            TriggerTimeInterval.ServerConfigureAndRegister(
                callback: ServerUpdate,
                name: "System." + this.ShortId,
                interval: TimeSpan.FromSeconds(10));
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

            var offlineSinceTime = privateState.ServerLastActiveTime;
            if (serverTime < offlineSinceTime + OfflineDurationToDespawn)
            {
                // despawn timeout is not reached yet
                return false;
            }

            if (LandClaimSystem.SharedIsOwnedLand(character.TilePosition, character, out _))
            {
                // do not despawn as the player is inside the owned land claim area
                return false;
            }

            return true;
        }

        private static void ServerUpdate()
        {
            var serverTime = Server.Game.FrameTime;
            foreach (var character in Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: false))
            {
                if (ServerIsNeedDespawn(character, serverTime))
                {
                    ServerCharacterDeathMechanic.DespawnCharacter(character);
                }
            }
        }
    }
}