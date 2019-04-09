namespace AtomicTorch.CBND.CoreMod.Systems.ServerWelcomeMessage
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class WelcomeMessageSystem : ProtoSystem<WelcomeMessageSystem>
    {
        public const string OfficialServerWelcomeMessage =
            @"Welcome to the official CryoFall server!
              [br]
              [br]
              [b]Important:[/b]
              Please remember that official servers are PvP centered and you might encounter aggression from other players. You will have to properly protect yourself and exercise vigilance at all times. You might also consider joining forces with other survivors to increase your chances.
              [br]
              [br]
              [b]Server rules:[/b]
              [*]Be respectful and courteous to others. We do not tolerate any form of abuse, discrimination, etc.
              [*]Comply with all requests by server moderators.
              [*]Do not spam or flood the chat or use excessive profanity.
              [br]
              [br]
              [b]Recommendations:[/b]
              [br]While we can't enforce these rules, following them would still create a nicer environment for everyone.
              [*]Don't KOS (kill on sight) other players, especially people who just joined.
              [*]If you are going to attack other player bases—that's perfectly fine, but at least don't grief them for no good reason.
              [*]Try to help new players who are still learning the game.
              [br]
              [br]Good luck! :)";

        public const string WelcomeToServerTitleFormat = "Welcome to {0}";

        private static readonly IClientStorage ClientStorageLastServerMessage;

        private static Task<string> lastGetWelcomeMessageTask;

        static WelcomeMessageSystem()
        {
            if (Api.IsClient)
            {
                ClientStorageLastServerMessage = Api.Client.Storage.GetStorage("Servers/LastWelcomeMessages");
                ClientStorageLastServerMessage.RegisterType(typeof(ServerAddress));
                ClientStorageLastServerMessage.RegisterType(typeof(AtomicGuid));
            }
        }

        public override string Name => "Welcome message system";

        public static async void ClientShowWelcomeMessage()
        {
            if (lastGetWelcomeMessageTask == null)
            {
                return;
            }

            var welcomeMessage = await lastGetWelcomeMessageTask;
            ClientShowWelcomeMessageInternal(welcomeMessage);
        }

        protected override void PrepareSystem()
        {
            if (IsServer)
            {
                return;
            }

            Client.Characters.CurrentPlayerCharacterChanged += Refresh;

            void Refresh()
            {
                if (Api.Client.Characters.CurrentPlayerCharacter != null)
                {
                    RefreshWelcomeMessage();
                }
            }
        }

        private static async void ClientShowWelcomeMessageInternal(string welcomeMessage)
        {
            if (string.IsNullOrWhiteSpace(welcomeMessage))
            {
                if (!Client.CurrentGame.IsConnectedToOfficialServer)
                {
                    return;
                }

                welcomeMessage = OfficialServerWelcomeMessage.Trim();
            }

            var game = Client.CurrentGame;
            var serverInfo = game.ServerInfo;

            await LoadingSplashScreenManager.WaitHiddenAsync();

            if (game.ConnectionState != ConnectionState.Connected
                || serverInfo != Client.CurrentGame.ServerInfo)
            {
                return;
            }

            var dialogWindow = DialogWindow.ShowDialog(
                string.Format(WelcomeToServerTitleFormat, serverInfo.ServerName),
                new ScrollViewer()
                {
                    MaxHeight = 380,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = new FormattedTextBlock()
                    {
                        Content = welcomeMessage,
                        TextWrapping = TextWrapping.Wrap,
                        TextTrimming = TextTrimming.None,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                },
                closeByEscapeKey: false);

            dialogWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            dialogWindow.GameWindow.FocusOnControl = null;
            dialogWindow.GameWindow.Width = 530;
            dialogWindow.GameWindow.UpdateLayout();
        }

        private static async void RefreshWelcomeMessage()
        {
            if (Api.IsEditor)
            {
                return;
            }

            var gameServerInfo = Client.CurrentGame.ServerInfo;

            var serverAddress = gameServerInfo.ServerAddress;
            lastGetWelcomeMessageTask = Instance.CallServer(_ => _.ServerRemote_GetWelcomeMessage());

            var welcomeMessage = await lastGetWelcomeMessageTask;
            if (string.IsNullOrWhiteSpace(welcomeMessage))
            {
                if (!Client.CurrentGame.IsConnectedToOfficialServer)
                {
                    return;
                }

                welcomeMessage = OfficialServerWelcomeMessage.Trim();
            }

            // load the last messages from storage
            if (!ClientStorageLastServerMessage.TryLoad(out Dictionary<ServerAddress, string> dictLastMessages))
            {
                dictLastMessages = new Dictionary<ServerAddress, string>();
            }

            if (dictLastMessages.TryGetValue(serverAddress, out var lastMessage)
                && lastMessage == welcomeMessage)
            {
                // the player already saw this welcome message
                return;
            }

            dictLastMessages[serverAddress] = welcomeMessage;
            ClientStorageLastServerMessage.Save(dictLastMessages);
            ClientShowWelcomeMessageInternal(welcomeMessage);
        }

        private string ServerRemote_GetWelcomeMessage()
        {
            return Server.Core.WelcomeMessageText;
        }
    }
}