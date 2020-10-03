namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using static GameApi.Scripting.Api;

    /// <summary>
    /// This system is storing the client blocked players list.
    /// </summary>
    public static class ClientChatBlockList
    {
        // {0} is the player name to block
        public const string DialogBlockPlayer_Format = "Are you sure you want to block {0}?";

        public const string DialogBlockPlayer_HowToUnblockLater =
            "You can unblock this player later using the social menu.";

        // {0} is the player name to unblock
        public const string DialogUnblockPlayer_Format = "Are you sure you want to unblock {0}?";

        private static HashSet<string> clientListBlocked;

        private static IClientStorage clientStorageListBlocked;

        private static bool isInitialized;

        public static event Action<(string name, bool isBlocked)> CharacterBlockStatusChanged;

        public static void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            ValidateIsClient();

            isInitialized = true;
            clientStorageListBlocked = Client.Storage.GetStorage(nameof(ClientChatBlockList));
            if (!clientStorageListBlocked.TryLoad(out clientListBlocked)
                || clientListBlocked is null)
            {
                clientListBlocked = new HashSet<string>();
            }
        }

        public static bool IsBlocked(string characterName)
        {
            return !string.IsNullOrEmpty(characterName)
                   && clientListBlocked.Contains(characterName);
        }

        public static void SetBlockStatus(string characterName, bool block, bool askConfirmation)
        {
            if (askConfirmation)
            {
                var message = string.Format(block
                                                ? DialogBlockPlayer_Format
                                                : DialogUnblockPlayer_Format,
                                            characterName);
                if (block)
                {
                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    message += "[br]" + DialogBlockPlayer_HowToUnblockLater;
                }

                DialogWindow.ShowDialog(
                    title: null,
                    message,
                    okText: CoreStrings.Yes,
                    okAction: () => SetBlockStatus(characterName,
                                                   block: block,
                                                   askConfirmation: false),
                    cancelAction: () => { },
                    focusOnCancelButton: true);
                return;
            }

            SetBlockStatus(characterName, block);
        }

        private static void SetBlockStatus(string characterName, bool block)
        {
            //if (characterName == ClientCurrentCharacterHelper.Character?.Name)
            //{
            //    // cannot block self
            //    block = false;
            //}

            var isChanged = block
                                ? clientListBlocked.Add(characterName)
                                : clientListBlocked.Remove(characterName);

            if (!isChanged)
            {
                return;
            }

            clientStorageListBlocked.Save(clientListBlocked);
            Logger.Important(
                $"Player {(block ? "blocked" : "unblocked")} player \"{characterName}\" for chat");

            SafeInvoke(() => CharacterBlockStatusChanged?.Invoke((characterName, block)));
        }
    }
}