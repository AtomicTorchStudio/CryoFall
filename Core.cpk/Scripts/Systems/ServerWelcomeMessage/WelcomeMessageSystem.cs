namespace AtomicTorch.CBND.CoreMod.Systems.ServerWelcomeMessage
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.ServerOperator;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.CurrentGame.Data;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class WelcomeMessageSystem : ProtoSystem<WelcomeMessageSystem>
    {
        public const string DescriptionModded =
            "This community server has third-party modifications installed. There is no guarantee that it will run correctly and offer you the proper CryoFall experience. If you encounter any issues—please direct them to this community server's owner.";

        public const string DescriptionPvE =
            @"This is a PvE (player versus environment) server. On this server, players cannot attack each other either directly or indirectly. This server is dedicated to peaceful exploration of the game without hostile player engagement. You cannot kill, destroy, steal or otherwise attack other players. Aside from that, the server offers you a complete CryoFall experience with its dangerous creatures, technologies to unlock, and world to explore. See where this journey will take you.
              [br]If you'd rather have a more thrilling and unrestricted experience—please consider joining a PvP server.";

        public const string DescriptionPvP =
            @"This is a free-for-all PvP server. Please remember that on PvP-centered servers, [b]you will encounter aggression[/b] from other players. You are expected to fight and defend yourself. You will have to properly protect yourself and exercise vigilance at all times. This game mode is only meant for experienced players.
              [br]If you'd rather have a more relaxed experience, especially if this is your first time playing—please consider joining a PvE server.";

        public const string HeaderDescriptionPvE = "PvE server";

        public const string HeaderDescriptionPvP = "PvP server";

        public const string HeaderModded = "Modded server";

        public const string HeaderWelcomeMessage = "Welcome message";

        public const string HeaderWipeInformation = "Wipe information";

        public const string WelcomeToServerTitleFormat = "Welcome to {0}";

        private const string ServerDatabaseScheduledWipeDateUtcPrefixAndKey = "ScheduledWipeDateUtc";

        private static readonly IClientStorage ClientStorageLastServerMessage;

        private static Task<WelcomeMessageRemoteData> lastGetWelcomeMessageTask;

        private static string serverDescriptionText;

        private static string serverWelcomeMessageText;

        static WelcomeMessageSystem()
        {
            if (Api.IsClient)
            {
                ClientStorageLastServerMessage = Api.Client.Storage.GetSessionStorage("Servers/LastWelcomeMessages");
                ClientStorageLastServerMessage.RegisterType(typeof(ServerAddress));
                ClientStorageLastServerMessage.RegisterType(typeof(AtomicGuid));
            }
        }

        public static DateTime? ServerScheduledWipeDateUtc { get; private set; }

        public override string Name => "Welcome message system";

        public static async void ClientEditDescription()
        {
            var originalText = await Instance.CallServer(_ => _.ServerRemote_GetDescriptionMessage());
            ClientOpenTextEditor(originalText,
                                 maxLength: 350,
                                 onSave: async text =>
                                         {
                                             await Instance.CallServer(
                                                 _ => _.ServerRemote_SetDescription(text));
                                             RefreshDescription();
                                         });
        }

        public static async void ClientEditScheduledWipeDate()
        {
            var data = await Instance.CallServer(_ => _.ServerRemote_GetInfo());

            var editWindow = new WindowScheduledWipeDateEditor();
            editWindow.SetSelectedDateUtc(data.ScheduledWipeDateUtc);
            editWindow.SaveAction = async nextWipeDate =>
                                    {
                                        if (data.ScheduledWipeDateUtc.Equals(nextWipeDate))
                                        {
                                            return;
                                        }

                                        await Instance.CallServer(
                                            _ => _.ServerRemote_SetScheduledWipeDate(nextWipeDate));
                                        RefreshWelcomeMessage();
                                    };

            Api.Client.UI.LayoutRootChildren.Add(editWindow);
        }

        public static async void ClientEditWelcomeMessage()
        {
            var welcomeMessageData = await Instance.CallServer(_ => _.ServerRemote_GetInfo());

            ClientOpenTextEditor(welcomeMessageData.WelcomeMessage,
                                 maxLength: 4096,
                                 onSave: async text =>
                                         {
                                             await Instance.CallServer(
                                                 _ => _.ServerRemote_SetWelcomeMessage(text));
                                             RefreshWelcomeMessage();
                                         });
        }

        public static async void ClientShowWelcomeMessage()
        {
            if (lastGetWelcomeMessageTask is null)
            {
                return;
            }

            var welcomeMessage = (await lastGetWelcomeMessageTask).WelcomeMessage;
            ClientShowWelcomeMessageInternal(welcomeMessage);
        }

        public static string FormatDate(DateTime dateTime)
        {
            var culture = CultureInfo.CurrentUICulture;
            return dateTime.ToString(culture.DateTimeFormat.ShortTimePattern)
                   + " "
                   + dateTime.ToString("dddd", culture) // day name
                   + " "
                   + dateTime.ToString(culture.DateTimeFormat.ShortDatePattern);
        }

        protected override void PrepareSystem()
        {
            if (IsServer)
            {
                serverWelcomeMessageText = TrimSpacesOnEachLine(Api.Server.Core.WelcomeMessageText);
                serverDescriptionText = TrimSpacesOnEachLine(Api.Server.Core.DescriptionMessageText);
                Api.Server.Database.TryGet(ServerDatabaseScheduledWipeDateUtcPrefixAndKey,
                                           ServerDatabaseScheduledWipeDateUtcPrefixAndKey,
                                           out DateTime? wipeDateUtc);

                if (wipeDateUtc.HasValue
                    && wipeDateUtc.Value < DateTime.Now)
                {
                    // the scheduled wipe date is in the past and no longer valid
                    wipeDateUtc = null;
                }

                ServerScheduledWipeDateUtc = wipeDateUtc;

                NextWipeDateServerTagHelper.ServerRefreshServerInfoTagForNextWipeDate();
                return;
            }

            Client.Characters.CurrentPlayerCharacterChanged += Refresh;

            void Refresh()
            {
                if (Api.Client.Characters.CurrentPlayerCharacter is not null)
                {
                    RefreshWelcomeMessage();
                }
            }
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        private static async Task<WelcomeMessageRemoteData> ClientOnWelcomeMessageReceivedFromServer(
            Task<WelcomeMessageRemoteData> t)
        {
            const string h1s = "[h1]",
                         h1c = "[/h1]",
                         br = "[br]",
                         brbr = br + br;

            var data = t.Result;
            var sb = new StringBuilder();

            var welcomeMessage = data.WelcomeMessage;
            await PveSystem.ClientAwaitPvEModeFromServer();

            // Header 1 - Server welcome message (section displayed only when available)
            if (!string.IsNullOrWhiteSpace(welcomeMessage))
            {
                sb.AppendLine(h1s + HeaderWelcomeMessage + h1c)
                  .AppendLine(br)
                  .AppendLine(welcomeMessage)
                  .AppendLine(brbr);
            }

            // Header 2 - PvP or PvE server description
            var isPveServer = PveSystem.ClientIsPve(logErrorIfDataIsNotYetAvailable: true);
            if (isPveServer)
            {
                sb.AppendLine(h1s + HeaderDescriptionPvE + h1c)
                  .AppendLine(br)
                  .AppendLine(DescriptionPvE)
                  .AppendLine(brbr);
            }
            else // PvP server
            {
                sb.AppendLine(h1s + HeaderDescriptionPvP + h1c)
                  .AppendLine(br)
                  .AppendLine(DescriptionPvP)
                  .AppendLine(brbr);
            }

            // Header 3 - Modded server (if the server is modded)
            if (Client.CurrentGame.ServerInfo.ModsOnServer.Count > 0)
            {
                sb.AppendLine(h1s + HeaderModded + h1c)
                  .AppendLine(br)
                  .AppendLine(DescriptionModded)
                  .AppendLine(brbr);
            }

            // Header 4 - Server wipe information          
            sb.AppendLine(h1s + HeaderWipeInformation + h1c)
              .AppendLine(br);

            if (data.ScheduledWipeDateUtc.HasValue)
            {
                sb.AppendLine(string.Format(CoreStrings.ServerWipeInfoStartedDate_Format,
                                            FormatDate(Client.CurrentGame.ServerInfo.CreationDateUtc.ToLocalTime())))
                  .AppendLine(br)
                  .AppendLine(string.Format(CoreStrings.ServerWipeInfoNextWipeDate_Format,
                                            FormatDate(data.ScheduledWipeDateUtc.Value.ToLocalTime())))
                  .AppendLine(br)
                  .AppendLine(string.Format(CoreStrings.TimeAlreadyConvertedToLocalTimeZone_Format,
                                            GetCurrentUtcOffsetText()));
            }
            else if (Client.CurrentGame.IsConnectedToOfficialServer
                     && isPveServer)
            {
                sb.AppendLine(CoreStrings.ServerWipeInfoOnMajorUpdatesOnly);
            }
            else
            {
                sb.AppendLine(CoreStrings.ServerWipeInfoNotSpecified
                              + " "
                              + CoreStrings.ConsultServerAdministrator);
            }

            return new WelcomeMessageRemoteData(sb.ToString(), data.ScheduledWipeDateUtc);
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        private static void ClientOpenTextEditor(
            string originalText,
            int maxLength,
            Action<string> onSave)
        {
            ScrollViewer scrollviewer;
            TextBox textBox;

            {
                scrollviewer = new ScrollViewer()
                {
                    Height = 530,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                };

                var grid = new Grid();
                scrollviewer.Content = grid;

                var textBlockInfo = new TextBlock()
                {
                    Text =
                        "Available tags: BB code like [b], [u], [i], [url=example.com]links[/url], [color=#FF0000]colors[/color], bullet points [*], and line breaks [br] (line breaks are added automatically for new lines).",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10, 0, 10, 0),
                    FontSize = 11,
                    LineHeight = 12,
                    LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                    FontWeight = FontWeights.Bold
                };

                originalText = originalText.Trim()
                                           .Replace("\r",   string.Empty)
                                           .Replace("\n",   string.Empty)
                                           .Replace("[br]", "\n")
                                           .Replace("[*]",  "\n[*]");

                textBox = new TextBox
                {
                    Text = originalText,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    FontSize = 13,
                    TextAlignment = TextAlignment.Left,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    AcceptsReturn = true,
                    Margin = default,
                    MaxLength = maxLength,
                    BorderThickness = default
                };

                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(7) });
                grid.RowDefinitions.Add(new RowDefinition());
                grid.Children.Add(textBlockInfo);
                Grid.SetRow(textBox, 2);
                grid.Children.Add(textBox);
            }

            var dialogWindow = DialogWindow.ShowDialog(
                title: null,
                content: scrollviewer,
                okAction: () =>
                          {
                              var text = textBox.Text
                                                .Trim()
                                                .Replace("\r",     string.Empty)
                                                .Replace("\n[br]", "\n")
                                                .Replace("\n[*]",  "[*]")
                                                .Replace("\n",     "\n[br]")
                                                .Replace("[*]",    "\n[*]");
                              onSave(text);
                          },
                cancelAction: () => { },
                okText: CoreStrings.Button_Save,
                focusOnCancelButton: true);

            dialogWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            dialogWindow.GameWindow.Width = 530;
        }

        private static async void ClientShowWelcomeMessageInternal(string welcomeMessage)
        {
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

        public static string GetCurrentUtcOffsetText()
        {
            var utcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            var result = utcOffset.ToString("hh\\:mm");
            if (utcOffset.TotalSeconds > 0)
            {
                result = "+" + result;
            }

            return result;
        }

        private static async void RefreshDescription()
        {
            var viewModel = ViewModelMenuCurrentGame.Instance;
            if (viewModel is null)
            {
                return;
            }

            viewModel.ServerDescription =
                await Instance.CallServer(_ => _.ServerRemote_GetDescriptionMessage());
        }

        private static async void RefreshWelcomeMessage()
        {
            if (Api.IsEditor
                && !ViewModelMenuCurrentGame.CurrentGameInfoEnabledInEditor)
            {
                return;
            }

            var serverAddress = Client.CurrentGame.ServerInfo.ServerAddress;
            lastGetWelcomeMessageTask = await Instance.CallServer(_ => _.ServerRemote_GetInfo())
                                                      .ContinueWith(ClientOnWelcomeMessageReceivedFromServer,
                                                                    TaskContinuationOptions.ExecuteSynchronously);

            var welcomeMessage = (await lastGetWelcomeMessageTask).WelcomeMessage;

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

        private static string TrimSpacesOnEachLine(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            text = text.Replace("\r", "");
            var split = text.Split('\n');
            for (var index = 0; index < split.Length; index++)
            {
                var line = split[index];
                line = line.Trim();
                split[index] = line;
            }

            text = string.Join(Environment.NewLine, split);
            return text;
        }

        [RemoteCallSettings(timeInterval: 1)]
        private string ServerRemote_GetDescriptionMessage()
        {
            return serverDescriptionText;
        }

        [RemoteCallSettings(timeInterval: 1)]
        private WelcomeMessageRemoteData ServerRemote_GetInfo()
        {
            if (ServerScheduledWipeDateUtc.HasValue
                && ServerScheduledWipeDateUtc.Value <= DateTime.Now)
            {
                // the scheduled wipe date is in the past and no longer valid
                ServerScheduledWipeDateUtc = null;
                NextWipeDateServerTagHelper.ServerRefreshServerInfoTagForNextWipeDate();
            }

            return new WelcomeMessageRemoteData(serverWelcomeMessageText, ServerScheduledWipeDateUtc);
        }

        [RemoteAuthorizeServerOperator]
        private bool ServerRemote_SetDescription(string text)
        {
            text = text?.Trim() ?? string.Empty;
            text = TrimSpacesOnEachLine(text);
            Api.Server.Core.DescriptionMessageText = serverDescriptionText = text;

            Logger.Important("Server description message changed to:"
                             + Environment.NewLine
                             + text.Replace("[br]", Environment.NewLine + "[br]"));
            return true;
        }

        [RemoteAuthorizeServerOperator]
        private bool ServerRemote_SetScheduledWipeDate(DateTime? dateTime)
        {
            if (dateTime.HasValue
                && dateTime.Value <= DateTime.Now)
            {
                throw new Exception("The scheduled wipe date cannot in the past");
            }

            ServerScheduledWipeDateUtc = dateTime;
            Api.Server.Database.Set(ServerDatabaseScheduledWipeDateUtcPrefixAndKey,
                                    ServerDatabaseScheduledWipeDateUtcPrefixAndKey,
                                    ServerScheduledWipeDateUtc);
            Logger.Important("Next scheduled wipe date changed: "
                             + (ServerScheduledWipeDateUtc.HasValue
                                    ? $"{ServerScheduledWipeDateUtc.Value.ToLongDateString()} at {ServerScheduledWipeDateUtc.Value.ToShortTimeString()}"
                                    : "none"),
                             ServerRemoteContext.Character);

            NextWipeDateServerTagHelper.ServerRefreshServerInfoTagForNextWipeDate();
            return true;
        }

        [RemoteAuthorizeServerOperator]
        private bool ServerRemote_SetWelcomeMessage(string text)
        {
            text = text?.Trim() ?? string.Empty;
            text = TrimSpacesOnEachLine(text);
            Api.Server.Core.WelcomeMessageText = serverWelcomeMessageText = text;

            Logger.Important("Server welcome message changed to:"
                             + Environment.NewLine
                             + text.Replace("[br]", Environment.NewLine + "[br]"));
            return true;
        }

        [NotPersistent]
        private readonly struct WelcomeMessageRemoteData : IRemoteCallParameter
        {
            public WelcomeMessageRemoteData(string welcomeMessage, DateTime? nextScheduledWipeDateUtc)
            {
                this.WelcomeMessage = welcomeMessage;
                this.ScheduledWipeDateUtc = nextScheduledWipeDateUtc;
            }

            public DateTime? ScheduledWipeDateUtc { get; }

            public string WelcomeMessage { get; }
        }
    }
}