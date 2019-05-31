namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using static GameApi.Scripting.Api;

    /// <summary>
    /// This system is storing the client blocked players list.
    /// </summary>
    public static class ClientChatBlockList
    {
        // {0} is "block" or "unblock" and {1} is player name
        public const string DialogBlockOrUnblockPlayer = "Are you sure you want to {0} {1}?";

        public const string DialogBlockPlayer_HowToUnblockLater =
            "You can unblock this player later using the social menu.";

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
                || clientListBlocked == null)
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
            if (characterName == ClientCurrentCharacterHelper.Character?.Name)
            {
                // cannot block self
                block = false;
            }

            if (askConfirmation)
            {
                var actionText = block
                                     ? CoreStrings.Chat_MessageMenu_Block
                                     : CoreStrings.Chat_MessageMenu_Unblock;

                var message = string.Format(DialogBlockOrUnblockPlayer,
                                            actionText,
                                            characterName);
                if (block)
                {
                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    message += "[br]" + DialogBlockPlayer_HowToUnblockLater;
                }

                DialogWindow.ShowDialog(
                    title: null,
                    message,
                    okText: actionText,
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
            if (characterName == ClientCurrentCharacterHelper.Character?.Name)
            {
                // cannot block self
                block = false;
            }

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