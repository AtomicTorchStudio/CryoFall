namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data;
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

        public void Hide(double delaySeconds)
        {
            if (this.isHiddenOrHiding)
            {
                return;
            }

            this.isHiddenOrHiding = true;
            var hideRequestId = ++this.lastHideRequestId;

            ClientComponentTimersManager.AddAction(
                delaySeconds,
                () =>
                {
                    if (this.isHiddenOrHiding
                        && hideRequestId == this.lastHideRequestId)
                    {
                        VisualStateManager.GoToElementState(this.textBlock, "Hidden", true);
                    }
                });
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

            this.MouseRightButtonUp += this.MouseRightButtonUpHandler;
            this.MouseLeave += this.MouseLeaveHandler;
            this.IsHitTestVisibleChanged += this.IsHitTestVisibleChangedHandler;
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            this.DestroyViewModel();

            this.MouseRightButtonUp -= this.MouseRightButtonUpHandler;
            this.MouseLeave -= this.MouseLeaveHandler;
            this.IsHitTestVisibleChanged -= this.IsHitTestVisibleChangedHandler;
        }

        private void DestroyContextMenu()
        {
            var m = this.ContextMenu;
            if (m == null)
            {
                return;
            }

            m.IsOpen = false;
            this.ContextMenu = null;
        }

        private void DestroyViewModel()
        {
            if (this.viewModel == null)
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
                this.DestroyContextMenu();
            }
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            this.DestroyContextMenu();
        }

        private void MouseRightButtonUpHandler(object sender, MouseEventArgs e)
        {
            if (!this.IsHitTestVisible)
            {
                return;
            }

            // create new context menu
            var contextMenu = new ContextMenu();
            var contextMenuItems = contextMenu.Items;

            if (this.viewModel.VisibilityCanMentionOrSendPrivateMessage == Visibility.Visible)
            {
                contextMenuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_Mention,
                        Command = this.viewModel.CommandMention
                    });

                contextMenuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_PrivateMessage,
                        Command = this.viewModel.CommandOpenPrivateChat
                    });
            }

            contextMenuItems.Add(
                new MenuItem()
                {
                    Header = CoreStrings.Copy,
                    Command = this.viewModel.CommandCopy,
                });

            if (this.viewModel.VisibilityCanInviteToParty == Visibility.Visible)
            {
                contextMenuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_InviteToParty,
                        Command = this.viewModel.CommandInviteToParty,
                    });
            }

            if (this.viewModel.VisibilityCanBlock == Visibility.Visible)
            {
                contextMenuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_Block,
                        Command = this.viewModel.CommandToggleBlock,
                    });
            }

            if (this.viewModel.VisibilityCanUnblock == Visibility.Visible)
            {
                contextMenuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_Unblock,
                        Command = this.viewModel.CommandToggleBlock,
                    });
            }

            this.ContextMenu = contextMenu;
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