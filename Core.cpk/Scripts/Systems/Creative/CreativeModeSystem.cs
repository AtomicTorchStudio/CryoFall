namespace AtomicTorch.CBND.CoreMod.Systems.Creative
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class CreativeModeSystem : ProtoSystem<CreativeModeSystem>
    {
        private static bool clientIsInCreativeMode;

        private static HashSet<string> serverUsersInCreativeMode;

        public event Action ClientCreativeModeChanged;

        public static IReadOnlyCollection<string> ServerUsersInCreativeMode
        {
            get
            {
                Api.ValidateIsServer();
                return serverUsersInCreativeMode;
            }
        }

        public override string Name => "Creative mode system";

        public static bool ClientIsInCreativeMode()
        {
            return SharedIsInCreativeMode(
                Client.Characters.CurrentPlayerCharacter);
        }

        public static void ClientRequestCurrentUserIsInCreativeMode()
        {
            Instance.CallServer(_ => _.ServerRemote_RequestCurrentUserIsInCreativeMode());
        }

        public static void ServerSetCreativeMode(ICharacter character, bool isCreativeModeEnabled)
        {
            if (character.IsNpc)
            {
                throw new Exception("Cannot promote NPC to creative mode");
            }

            var isChanged = isCreativeModeEnabled
                                ? serverUsersInCreativeMode.Add(character.Name)
                                : serverUsersInCreativeMode.Remove(character.Name);

            if (!isChanged)
            {
                Logger.Warning(
                    $"Server creative mode is NOT changed: {character} - creative mode is {(isCreativeModeEnabled ? "enabled" : "disabled")}");
                return;
            }

            Logger.Warning(
                $"Server creative mode is changed: {character} - creative mode is {(isCreativeModeEnabled ? "enabled" : "disabled")}");

            ServerLogCharactersList();

            // notify that character
            Instance.CallClient(
                character,
                _ => _.ClientRemote_SetCurrentUserIsInCreativeMode(isCreativeModeEnabled));
        }

        public static bool SharedIsInCreativeMode(ICharacter character)
        {
            if (character is null)
            {
                return false;
            }

            if (IsServer)
            {
                return serverUsersInCreativeMode.Contains(character.Name);
            }

            return clientIsInCreativeMode
                   && Client.Characters.CurrentPlayerCharacter == character;
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                clientIsInCreativeMode = false;
                return;
            }

            // below is the Server-side code only
            if (Api.Server.Database.TryGet(
                "creative",
                "CreativeModeCharactersIdsList",
                out serverUsersInCreativeMode))
            {
                ServerLogCharactersList();
                return;
            }

            // characters list is not stored
            serverUsersInCreativeMode = new HashSet<string>(StringComparer.Ordinal);
            Api.Server.Database.Set(
                "creative",
                "CreativeModeCharactersIdsList",
                serverUsersInCreativeMode);

            ServerLogCharactersList();
        }

        private static void ServerLogCharactersList()
        {
            Logger.Important("Creative mode - characters in creative mode: "
                             + serverUsersInCreativeMode.GetJoinedString());
        }

        private void ClientRemote_SetCurrentUserIsInCreativeMode(bool isInCreativeMode)
        {
            clientIsInCreativeMode = isInCreativeMode;
            Logger.Important("Received isInCreativeMode=" + isInCreativeMode);
            Api.SafeInvoke(() => this.ClientCreativeModeChanged?.Invoke());
        }

        private void ServerRemote_RequestCurrentUserIsInCreativeMode()
        {
            var character = ServerRemoteContext.Character;
            if (Api.IsEditor)
            {
                // automatically enable creative mode for current player in Editor
                // (and do this only once)
                if (!Api.Server.Database.TryGet("creative", "EditorCreativeModeFirstTimeSet", out bool _))
                {
                    Api.Server.Database.Set("creative", "EditorCreativeModeFirstTimeSet", true);
                    ServerSetCreativeMode(character, isCreativeModeEnabled: true);
                }
            }

            var isInCreativeMode = SharedIsInCreativeMode(character);
            this.CallClient(character, _ => _.ClientRemote_SetCurrentUserIsInCreativeMode(isInCreativeMode));
        }
    }
}