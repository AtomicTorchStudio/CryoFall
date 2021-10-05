namespace AtomicTorch.CBND.CoreMod.Systems.PlayerReportSystem
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class PlayerReportSystem : ProtoSystem<PlayerReportSystem>
    {
        public const string DialogReport_AreYouSure
            = "Are you sure you want to report the following message?";

        public const string DialogReport_CommunityServerDisclaimer
            = "[b]Please note:[/b] You're playing on a community server, so the chat moderation is the responsibility of the server owner. Usually chats are not moderated.";

        public const string DialogReport_OfficialServersDisclaimer
            = "[b]Please note:[/b] Official servers are NOT moderated.";

        public const string DialogReport_Title = "Report a message";

        private const string DatabaseKeyReportedChatEntries = "ReportedChatEntries";

        private const int MaxChatReportEntries = 250;

        private static List<ReportedChatEntry> serverReportedChatEntries;

        [NotLocalizable]
        public override string Name => "Player report system";

        public static void ClientReportChatEntry(ChatEntry chatEntry)
        {
            var name = chatEntry.From;
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            DialogWindow.ShowDialog(
                title: DialogReport_Title,
                text: DialogReport_AreYouSure
                      + "[br]"
                      + "[br]"
                      + $"\"{name}: {chatEntry.ClientGetFilteredMessage()}\""
                      + "[br]"
                      + "[br]"
                      + (Api.Client.CurrentGame.IsConnectedToOfficialServer
                             ? DialogReport_OfficialServersDisclaimer
                             : DialogReport_CommunityServerDisclaimer),
                okText: CoreStrings.Chat_MessageMenu_Report,
                okAction: () =>
                          {
                              // auto-block the offender
                              ClientChatBlockList.SetBlockStatus(name,
                                                                 block: true,
                                                                 askConfirmation: false);

                              Instance.CallServer(_ => _.ServerRemote_ReportChatEntry(chatEntry));
                          },
                cancelText: CoreStrings.Button_Cancel,
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            var database = Server.Database;
            if (!database.TryGet(nameof(PlayerReportSystem),
                                 DatabaseKeyReportedChatEntries,
                                 out serverReportedChatEntries))
            {
                serverReportedChatEntries = new List<ReportedChatEntry>(capacity: MaxChatReportEntries);
                database.Set(nameof(PlayerReportSystem), DatabaseKeyReportedChatEntries, serverReportedChatEntries);
            }
        }

        [RemoteCallSettings(timeInterval: 5)]
        private void ServerRemote_ReportChatEntry(ChatEntry chatEntry)
        {
            var reporterCharacter = ServerRemoteContext.Character;
            foreach (var entry in serverReportedChatEntries)
            {
                if (entry.ChatEntry.Equals(chatEntry))
                {
                    // this chat entry is already reported
                    return;
                }
            }

            Logger.Important("Chat entry reported: "
                             + chatEntry.From
                             + " - "
                             + chatEntry.UtcDate.ToLongDateString()
                             + " - "
                             + chatEntry.Message,
                             reporterCharacter);

            while (serverReportedChatEntries.Count + 1 > MaxChatReportEntries)
            {
                // trim the log
                serverReportedChatEntries.RemoveAt(0);
            }

            serverReportedChatEntries.Add(
                new ReportedChatEntry(chatEntry,
                                      reporterCharacter.Id,
                                      reporterCharacter.Name,
                                      isReportRead: false));
        }
    }
}