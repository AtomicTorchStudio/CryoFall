namespace AtomicTorch.CBND.CoreMod.Systems.CharacterInvincibility
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class CharacterInvincibilitySystem : ProtoSystem<CharacterInvincibilitySystem>
    {
        private const string DatabaseEntryId = "ServerInvincibleCharactersIdsList";

        private static HashSet<string> serverInvicibleCharactersList;

        public static void ServerAdd(ICharacter character)
        {
            if (character.IsNpc)
            {
                throw new Exception("NPC cannot be invincible");
            }

            ServerAdd(character.Name);
        }

        public static void ServerAdd(string name)
        {
            ServerSetInvincibilityMode(name, isInvincible: true);
        }

        public static bool ServerIsInvincible(ICharacter character)
        {
            if (character is null
                || character.IsNpc)
            {
                return false;
            }

            Api.ValidateIsServer();
            return serverInvicibleCharactersList.Contains(character.Name);
        }

        public static void ServerRemove(ICharacter character)
        {
            if (character.IsNpc)
            {
                throw new Exception("NPC cannot be invincible");
            }

            ServerRemove(character.Name);
        }

        public static void ServerRemove(string name)
        {
            ServerSetInvincibilityMode(name, isInvincible: false);
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            // below is the Server-side code only
            if (Server.Database.TryGet(
                nameof(ServerOperatorSystem),
                DatabaseEntryId,
                out serverInvicibleCharactersList))
            {
                ServerLogCharactersList();
                return;
            }

            // invincible list is not stored, create a new one
            serverInvicibleCharactersList = new HashSet<string>(StringComparer.Ordinal);
            Server.Database.Set(
                nameof(ServerOperatorSystem),
                DatabaseEntryId,
                serverInvicibleCharactersList);

            ServerLogCharactersList();
        }

        private static void ServerLogCharactersList()
        {
            Logger.Important(
                "Server invincible characters system characters list: "
                + serverInvicibleCharactersList.GetJoinedString());
        }

        private static void ServerSetInvincibilityMode(string name, bool isInvincible)
        {
            var isChanged = isInvincible
                                ? serverInvicibleCharactersList.Add(name)
                                : serverInvicibleCharactersList.Remove(name);

            if (!isChanged)
            {
                return;
            }

            Logger.Warning(string.Format("Server invincible mode is changed: \"{0}\" - invincible mode is {1}",
                                         name,
                                         isInvincible ? "enabled" : "disabled"));

            ServerLogCharactersList();
        }
    }
}