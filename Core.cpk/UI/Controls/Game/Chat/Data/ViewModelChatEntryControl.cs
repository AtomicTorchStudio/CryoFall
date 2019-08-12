namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
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

        private static Brush brushFromCurrentPlayer;

        private static Brush brushFromOtherPlayer;

        private static Brush brushFromPartyMember;

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

            if (brushFromCurrentPlayer == null)
            {
                brushFromCurrentPlayer = (Brush)resources.TryFindResource("ChatBrushFromCurrentPlayer");
                brushFromOtherPlayer = (Brush)resources.TryFindResource("ChatBrushFromOtherPlayer");
                brushFromPartyMember = (Brush)resources.TryFindResource("ChatBrushFromPartyMember");
            }

            this.UpdateText(inlines);
        }

        public BaseCommand CommandCopy => new ActionCommand(this.ExecuteCommandCopy);

        public BaseCommand CommandCopyName => new ActionCommand(this.ExecuteCommandCopyName);

        public BaseCommand CommandInviteToParty => new ActionCommand(this.ExecuteCommandInviteToParty);

        public BaseCommand CommandMention => new ActionCommand(this.ExecuteCommandMention);

        public BaseCommand CommandOpenPrivateChat => new ActionCommand(this.ExecuteCommandOpenPrivateChat);

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

        public Visibility VisibilityCanInviteToParty
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
                    // cannot invite self
                    return Visibility.Collapsed;
                }

                if (PartySystem.ClientIsPartyMember(name))
                {
                    // already in party
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
            base.DisposeViewModel();
            ClientChatBlockList.CharacterBlockStatusChanged -= this.CharacterBlockStatusChangedHandler;
        }

        private void CharacterBlockStatusChangedHandler((string name, bool isBlocked) obj)
        {
            this.NotifyPropertyChanged(nameof(this.VisibilityCanBlock));
            this.NotifyPropertyChanged(nameof(this.VisibilityCanUnblock));
        }

        private void ExecuteCommandCopy()
        {
            var name = this.chatEntry.From;
            var text = string.IsNullOrEmpty(name)
                           ? this.chatEntry.Message
                           : string.Format(ChatCopyMessageFormat, name, this.chatEntry.Message);

            Api.Client.Core.CopyToClipboard(text);
        }

        private void ExecuteCommandCopyName()
        {
            var name = this.chatEntry.From;
            Api.Client.Core.CopyToClipboard(name);
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
            // convert timestamp of the chat entry to the local DateTime
            var date = TimeZone.CurrentTimeZone.ToLocalTime(this.chatEntry.UtcDate);
            inlines.Add(new Run(date.ToString(ShortTimePattern)
                                    .Replace(' ', NoBreakSpace)
                                + NoBreakSpace)
                            { FontWeight = FontWeights.Light });

            var name = this.chatEntry.From;
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

            if (isServiceMessage)
            {
                inlines.Add(new Run(this.chatEntry.Message));
            }
            else
            {
                inlines.Add(new Run(":" + NoBreakSpace + this.chatEntry.Message));
            }

            var isFromCurrentPlayer = Api.Client.Characters.CurrentPlayerCharacter.Name?.Equals(name)
                                      ?? false;

            var isPartyMember = !isFromCurrentPlayer
                                && PartySystem.ClientIsPartyMember(name);

            this.Foreground = isFromCurrentPlayer
                                  ? brushFromCurrentPlayer
                                  : isPartyMember
                                      ? brushFromPartyMember
                                      : brushFromOtherPlayer;
        }
    }
}