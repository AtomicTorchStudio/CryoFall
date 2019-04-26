namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelChatRoom : BaseViewModel
    {
        public readonly BaseChatRoom ChatRoom;

        private readonly Action callbackNeedTabSort;

        private bool isOpened;

        private bool isSelected;

        public ViewModelChatRoom(BaseChatRoom chatRoom, Action callbackNeedTabSort)
        {
            this.ChatRoom = chatRoom;
            this.callbackNeedTabSort = callbackNeedTabSort;
            chatRoom.ClientMessageAdded += this.ChatRoomClientMessageAddedHandler;
        }

        public bool HasUnreadMessages { get; set; }

        public bool IsOpened
        {
            get => this.isOpened;
            set
            {
                if (this.isOpened == value)
                {
                    return;
                }

                this.isOpened = value;
                this.NotifyThisPropertyChanged();

                this.TryResetUnreadFlag();
            }
        }

        /// <summary>
        /// Returns true if this is the currently selected tab.
        /// Bound via XAML from chat panel control.
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;
                this.NotifyThisPropertyChanged();

                this.TryResetUnreadFlag();
            }
        }

        public bool IsTabVisible { get; set; }

        public string Title => this.ChatRoom.ClientGetTitle();

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.ChatRoom.ClientMessageAdded -= this.ChatRoomClientMessageAddedHandler;
        }

        private void ChatRoomClientMessageAddedHandler(ChatEntry chatEntry)
        {
            if (this.ChatRoom is ChatRoomPrivate)
            {
                this.callbackNeedTabSort();
            }

            if (chatEntry.IsService)
            {
                return;
            }

            this.HasUnreadMessages = true;
            this.IsTabVisible = this.HasUnreadMessages;
        }

        private void TryResetUnreadFlag()
        {
            if (this.isSelected
                && this.IsOpened)
            {
                this.HasUnreadMessages = false;
            }
        }
    }
}