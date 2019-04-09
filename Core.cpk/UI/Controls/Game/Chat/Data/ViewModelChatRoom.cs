namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelChatRoom : BaseViewModel
    {
        public readonly BaseChatRoom ChatRoom;

        private bool isSelected;

        public ViewModelChatRoom(BaseChatRoom chatRoom, Action callbackNeedTabSort)
        {
            this.ChatRoom = chatRoom;
            this.callbackNeedTabSort = callbackNeedTabSort;
            chatRoom.ClientMessageAdded += this.ChatRoomClientMessageAddedHandler;
        }

        private readonly Action callbackNeedTabSort;

        public bool HasUnreadMessages { get; set; }

        public bool IsOpened { get; set; }

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

                if (this.isSelected)
                {
                    this.HasUnreadMessages = false;
                }
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

            if (this.IsSelected
                || chatEntry.IsService)
            {
                return;
            }

            this.HasUnreadMessages = true;
            this.IsTabVisible = this.HasUnreadMessages;
        }
    }
}