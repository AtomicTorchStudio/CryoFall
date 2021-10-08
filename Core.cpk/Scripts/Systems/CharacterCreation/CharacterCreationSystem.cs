namespace AtomicTorch.CBND.CoreMod.Systems.CharacterCreation
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterOrigins;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterRespawn;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterStyle;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class CharacterCreationSystem : ProtoSystem<CharacterCreationSystem>
    {
        public static event Action<ICharacter> ServerCharacterCreated;

        public static bool SharedIsEnabled => !Api.IsEditor;

        public static void ClientSelectOrigin(ProtoCharacterOrigin origin)
        {
            Instance.CallServer(_ => _.ServerRemote_SelectOrigin(origin));
        }

        public static bool SharedIsCharacterCreated(ICharacter character)
        {
            var privateState = PlayerCharacter.GetPrivateState(character);
            return privateState.IsAppearanceSelected
                   && privateState.Origin is not null;
        }

        private static void ServerCharacterAppearanceSelectedHandler(ICharacter character)
        {
            var privateState = PlayerCharacter.GetPrivateState(character);
            if (privateState.IsAppearanceSelected)
            {
                return;
            }

            // character appearance selected for the first time
            privateState.IsAppearanceSelected = true;
            Logger.Info("Character appearance selected");

            ServerSpawnIfReady(character);
        }

        /// <summary>
        /// Please do not invoke for the already spawned character.
        /// </summary>
        public static void ServerSpawnIfReady(ICharacter character)
        {
            if (!SharedIsCharacterCreated(character))
            {
                return;
            }

            Api.SafeInvoke(() => ServerCharacterCreated?.Invoke(character));
            CharacterRespawnSystem.ServerOnCharacterCreated(character);
        }

        private void ServerRemote_SelectOrigin(ProtoCharacterOrigin origin)
        {
            Api.Assert(origin is not null, "No origin provided");

            var character = ServerRemoteContext.Character;
            var privateState = PlayerCharacter.GetPrivateState(character);
            Api.Assert(privateState.Origin is null, "Origin already selected, cannot change it");

            ServerSetOrigin(character, origin);
            ServerSpawnIfReady(character);
        }

        public static void ServerSetOrigin(ICharacter character, ProtoCharacterOrigin origin)
        {
            var privateState = PlayerCharacter.GetPrivateState(character);
            privateState.Origin = origin;
            privateState.SetFinalStatsCacheIsDirty();
        }

        private class Bootstrappper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                base.ServerInitialize(serverConfiguration);

                CharacterStyleSystem.ServerCharacterAppearanceSelected
                    += ServerCharacterAppearanceSelectedHandler;

                foreach (var character in Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: false))
                {
                    var privateState = PlayerCharacter.GetPrivateState(character);
                    if (!privateState.IsDespawned
                        && !SharedIsCharacterCreated(character))
                    {
                        // despawn all invalid characters
                        CharacterDespawnSystem.DespawnCharacter(character);
                    }
                }
            }
        }
    }
}