namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public partial class ChatEntryControl : BaseUserControl
    {
        private ChatEntry chatEntry;

        private ChatRoomControl chatRoomControl;

        private bool isHiddenOrHiding;

        private byte lastHideRequestId;

        private TextBlock textBlock;

        private ViewModelChatEntryControl viewModel;

        public ViewModelChatEntryControl ViewModel => this.viewModel;

        public void Hide(double delaySeconds)
        {
            if (this.isHiddenOrHiding)
            {
                return;
            }

            this.isHiddenOrHiding = true;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            var hideRequestId = ++this.lastHideRequestId;

            ClientTimersSystem.AddAction(delaySeconds, HideAfterDelay);

            void HideAfterDelay()
            {
                if (this.isHiddenOrHiding
                    && hideRequestId == this.lastHideRequestId)
                {
                    VisualStateManager.GoToElementState(this.textBlock, "Hidden", true);
                }
            }
        }

        public void Setup(ChatRoomControl chatRoomControl, ChatEntry value)
        {
            this.chatRoomControl = chatRoomControl;
            if (this.chatEntry.Equals(value))
            {
                return;
            }

            this.chatEntry = value;
            this.UpdateViewModel();
        }

        public void Show()
        {
            if (this.isHiddenOrHiding)
            {
                this.isHiddenOrHiding = false;
                this.HorizontalAlignment = HorizontalAlignment.Stretch;
            }

            VisualStateManager.GoToElementState(this.textBlock, "Default", false);
        }

        protected override void InitControl()
        {
            base.InitControl();
            this.textBlock = this.GetByName<TextBlock>("TextBlock");
            if (IsDesignTime)
            {
                this.textBlock.Text = "Username: message text";
            }
        }

        protected override void OnLoaded()
        {
            this.UpdateViewModel();
            VisualStateManager.GoToElementState(this.textBlock, "Default", false);

            this.MouseUp += this.MouseUpHandler;
            this.IsHitTestVisibleChanged += this.IsHitTestVisibleChangedHandler;
        }

        protected override void OnUnloaded()
        {
            this.DestroyViewModel();

            this.MouseUp -= this.MouseUpHandler;
            this.IsHitTestVisibleChanged -= this.IsHitTestVisibleChangedHandler;
        }

        private void DestroyViewModel()
        {
            if (this.viewModel is null)
            {
                return;
            }

            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private void IsHitTestVisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool)e.NewValue;
            if (!isVisible)
            {
                ClientContextMenuHelper.CloseLastContextMenuFor(this);
            }
        }

        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (!this.IsHitTestVisible)
            {
                return;
            }

            var menuItems = new List<MenuItem>();
            var chatEntryFrom = this.viewModel.ChatEntry.From;

            var canMentionOrSendPrivateMessage =
                this.viewModel.VisibilityCanMentionOrSendPrivateMessage == Visibility.Visible;
            if (canMentionOrSendPrivateMessage)
            {
                menuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_Mention,
                        Command = this.viewModel.CommandMention
                    });
            }

            menuItems.Add(
                new MenuItem()
                {
                    Header = CoreStrings.Copy,
                    Command = this.viewModel.CommandCopy
                });

            if (!string.IsNullOrEmpty(this.viewModel.ChatEntry.From))
            {
                menuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_CopyName,
                        Command = this.viewModel.CommandCopyName
                    });
            }

            if (canMentionOrSendPrivateMessage)
            {
                menuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_PrivateMessage,
                        Command = this.viewModel.CommandOpenPrivateChat
                    });
            }

            if (PartySystem.ClientCanInvite(chatEntryFrom))
            {
                menuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_InviteToParty,
                        Command = this.viewModel.CommandInviteToParty
                    });
            }

            if (FactionSystem.ClientCanInviteToFaction(chatEntryFrom))
            {
                menuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Faction_InviteToFaction,
                        Command = this.viewModel.CommandInviteToFaction
                    });
            }

            if (this.viewModel.VisibilityCanBlock == Visibility.Visible)
            {
                menuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_Block,
                        Command = this.viewModel.CommandToggleBlock
                    });

                menuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_Report,
                        Command = this.viewModel.CommandReport
                    });
            }

            if (this.viewModel.VisibilityCanUnblock == Visibility.Visible)
            {
                menuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_Unblock,
                        Command = this.viewModel.CommandToggleBlock
                    });
            }

            ClientContextMenuHelper.ShowMenuOnClick(this, menuItems);
        }

        private void UpdateViewModel()
        {
            this.DestroyViewModel();

            if (!this.isLoaded)
            {
                return;
            }

            this.viewModel = new ViewModelChatEntryControl(this.chatRoomControl,
                                                           this.chatEntry,
                                                           this.textBlock.Inlines,
                                                           this);
            this.DataContext = this.viewModel;
        }
    }
}