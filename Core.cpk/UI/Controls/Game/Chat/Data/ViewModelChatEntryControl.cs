namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.PlayerReportSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelChatEntryControl : BaseViewModel
    {
        // {0} is player name (who sent the chat message), {1} is the message text
        public const string ChatCopyMessageFormat = "From @{0}: {1}";

        public const string ChatNamePrefix_Developer = "[Developer]";

        private const char NoBreakSpace = '\u00A0';

        private static readonly string ShortTimePattern
            = CultureInfo.InstalledUICulture.DateTimeFormat.ShortTimePattern;

        // vector image for supporter badge
        private static readonly Geometry SupporterBadgeGeometry
            = Api.Client.UI.GetApplicationResource<Geometry>("IconGeometrySupporterBadge");

        private static Brush brushFromCurrentPlayer;

        private static Brush brushFromOtherPlayer;

        private static Brush brushFromPartyOrFactionMember;

        private readonly ChatEntry chatEntry;

        private readonly ChatRoomControl chatRoomControl;

        public ViewModelChatEntryControl(
            ChatRoomControl chatRoomControl,
            ChatEntry chatEntry,
            InlineCollection inlines,
            FrameworkElement resources)
        {
            this.chatRoomControl = chatRoomControl;
            this.chatEntry = chatEntry;
            ClientChatBlockList.CharacterBlockStatusChanged += this.CharacterBlockStatusChangedHandler;

            if (brushFromCurrentPlayer is null)
            {
                brushFromCurrentPlayer = (Brush)resources.TryFindResource("ChatBrushFromCurrentPlayer");
                brushFromOtherPlayer = (Brush)resources.TryFindResource("ChatBrushFromOtherPlayer");
                brushFromPartyOrFactionMember = (Brush)resources.TryFindResource("ChatBrushFromPartyOrFactionMember");
            }

            this.UpdateText(inlines);
        }

        public ChatEntry ChatEntry => this.chatEntry;

        public BaseCommand CommandCopy => new ActionCommand(this.ExecuteCommandCopy);

        public BaseCommand CommandCopyName => new ActionCommand(this.ExecuteCommandCopyName);

        public BaseCommand CommandInviteToFaction => new ActionCommand(this.ExecuteCommandInviteToFaction);

        public BaseCommand CommandInviteToParty => new ActionCommand(this.ExecuteCommandInviteToParty);

        public BaseCommand CommandMention => new ActionCommand(this.ExecuteCommandMention);

        public BaseCommand CommandOpenPrivateChat => new ActionCommand(this.ExecuteCommandOpenPrivateChat);

        public BaseCommand CommandReport => new ActionCommand(this.ExecuteCommandReport);

        public BaseCommand CommandToggleBlock => new ActionCommand(this.ExecuteCommandToggleBlock);

        public Brush Foreground { get; private set; }

        public Visibility VisibilityCanBlock
        {
            get
            {
                var name = this.chatEntry.From;
                if (string.IsNullOrEmpty(name))
                {
                    // not character related
                    return Visibility.Collapsed;
                }

                if (name == Api.Client.Characters.CurrentPlayerCharacter?.Name)
                {
                    // cannot block self
                    return Visibility.Collapsed;
                }

                if (ClientChatBlockList.IsBlocked(name))
                {
                    // already blocked
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

        public Visibility VisibilityCanMentionOrSendPrivateMessage
        {
            get
            {
                var name = this.chatEntry.From;
                if (string.IsNullOrEmpty(name))
                {
                    // not character related
                    return Visibility.Collapsed;
                }

                if (name == Api.Client.Characters.CurrentPlayerCharacter?.Name)
                {
                    // cannot PM self
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

        public Visibility VisibilityCanUnblock
        {
            get
            {
                var name = this.chatEntry.From;
                if (string.IsNullOrEmpty(name))
                {
                    // not character related
                    return Visibility.Collapsed;
                }

                if (name == Api.Client.Characters.CurrentPlayerCharacter?.Name)
                {
                    // cannot unblock self
                    return Visibility.Collapsed;
                }

                if (!ClientChatBlockList.IsBlocked(name))
                {
                    // already unblocked
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

        protected override void DisposeViewModel()
        {
            ClientChatBlockList.CharacterBlockStatusChanged -= this.CharacterBlockStatusChangedHandler;
            base.DisposeViewModel();
        }

        private static void AddSupporterBadge(InlineCollection inlines)
        {
            // hidden textblock is acting as a source of the foreground brush for the icon
            var hiddenTextBlock = new TextBlock() { Visibility = Visibility.Hidden };

            var iconPath = new Path
            {
                Width = 12,
                Height = 12,
                Data = SupporterBadgeGeometry,
                Stretch = Stretch.Uniform
            };
            iconPath.SetBinding(Shape.FillProperty,
                                // bind to the foreground of the hidden textblock
                                new Binding("Foreground") { Source = hiddenTextBlock });

            var grid = new Grid()
            {
                Background = Brushes.Transparent,
                Margin = new Thickness(1, 0, 0, -4)
            };

            grid.Children.Add(hiddenTextBlock);
            grid.Children.Add(iconPath);
            inlines.Add(new InlineUIContainer(grid));

            ToolTipServiceExtend.SetToolTip(grid,
                                            new FormattedTextBlock()
                                            {
                                                Content = "[b]"
                                                          + CoreStrings.SupporterPack_Badge
                                                          + "[/b][br]"
                                                          + CoreStrings.SupporterPack_Description,
                                                MaxWidth = 300
                                            });
        }

        private void CharacterBlockStatusChangedHandler((string name, bool isBlocked) obj)
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.NotifyPropertyChanged(nameof(this.VisibilityCanBlock));
            this.NotifyPropertyChanged(nameof(this.VisibilityCanUnblock));
        }

        private void ExecuteCommandCopy()
        {
            var name = this.chatEntry.From;
            var text = string.IsNullOrEmpty(name)
                           ? this.chatEntry.ClientGetFilteredMessage()
                           : string.Format(ChatCopyMessageFormat, name, this.chatEntry.ClientGetFilteredMessage());

            Api.Client.Core.CopyToClipboard(text);
        }

        private void ExecuteCommandCopyName()
        {
            var name = this.chatEntry.From;
            Api.Client.Core.CopyToClipboard(name);
        }

        private void ExecuteCommandInviteToFaction()
        {
            FactionSystem.ClientOfficerInviteMember(this.chatEntry.From);
        }

        private void ExecuteCommandInviteToParty()
        {
            PartySystem.ClientInviteMember(this.chatEntry.From);
        }

        private void ExecuteCommandMention()
        {
            this.chatRoomControl.AddMention(this.chatEntry.From);
        }

        private void ExecuteCommandOpenPrivateChat()
        {
            ChatSystem.ClientOpenPrivateChat(withCharacterName: this.chatEntry.From);
        }

        private void ExecuteCommandReport()
        {
            PlayerReportSystem.ClientReportChatEntry(this.chatEntry);
        }

        private void ExecuteCommandToggleBlock()
        {
            var name = this.chatEntry.From;
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var isBlocked = ClientChatBlockList.IsBlocked(name);
            ClientChatBlockList.SetBlockStatus(name,
                                               block: !isBlocked,
                                               askConfirmation: true);
        }

        private void UpdateText(InlineCollection inlines)
        {
            var name = this.chatEntry.From;
            var isFromCurrentPlayer = Api.Client.Characters.CurrentPlayerCharacter.Name?.Equals(name)
                                      ?? false;

            if (isFromCurrentPlayer)
            {
                this.Foreground = brushFromCurrentPlayer;
            }
            else
            {
                this.Foreground = PartySystem.ClientIsPartyMember(name)
                                  || FactionSystem.ClientIsFactionMember(name)
                                      ? brushFromPartyOrFactionMember
                                      : brushFromOtherPlayer;
            }

            // convert timestamp of the chat entry to the local DateTime
            var date = this.chatEntry.UtcDate.ToLocalTime();
            inlines.Add(new Run(date.ToString(ShortTimePattern)
                                    .Replace(' ', NoBreakSpace)
                                + NoBreakSpace)
                            { FontWeight = FontWeights.Light });

            if (!this.chatEntry.IsService
                && this.chatEntry.HasSupporterPack)
            {
                AddSupporterBadge(inlines);
            }

            var isServiceMessage = this.chatEntry.IsService;

            if (!isServiceMessage)
            {
                var dispayedName = name;
                if (DevelopersListHelper.IsDeveloper(dispayedName))
                {
                    dispayedName = ChatNamePrefix_Developer + NoBreakSpace + dispayedName;
                }

                inlines.Add(new Run(dispayedName) { FontWeight = FontWeights.SemiBold });
            }

            var message = this.chatEntry.ClientGetFilteredMessage();
            inlines.Add(new Run(isServiceMessage
                                    ? message
                                    : ":" + NoBreakSpace + message));
        }
    }
}