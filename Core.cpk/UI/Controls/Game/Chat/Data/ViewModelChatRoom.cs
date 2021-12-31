namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelChatRoom : BaseViewModel
    {
        public const string DialogClosePrivateChat_Format =
            @"Do you want to close the private chat with {0}?
              [br]The chat history will be saved and you can reopen the chat later.";

        public readonly BaseChatRoom ChatRoom;

        private readonly Action callbackNeedTabSort;

        private readonly Action<ViewModelChatRoom> callbackPrivateChatRoomClosed;

        private bool isOpened;

        private bool isPanelAllowsTabVisibility;

        private bool isSelected;

        public ViewModelChatRoom(
            BaseChatRoom chatRoom,
            Action callbackNeedTabSort,
            Action<ViewModelChatRoom> callbackPrivateChatRoomClosed)
        {
            this.ChatRoom = chatRoom;
            this.callbackNeedTabSort = callbackNeedTabSort;
            this.callbackPrivateChatRoomClosed = callbackPrivateChatRoomClosed;
            chatRoom.ClientMessageAdded += this.ChatRoomClientMessageAddedHandler;

            if (chatRoom is ChatRoomPrivate privateChat)
            {
                if (privateChat.ClientIsCurrentCharacterA())
                {
                    privateChat.ClientSubscribe(
                        _ => _.IsClosedByCharacterA,
                        _ => this.RefreshTabVisibility(),
                        this);

                    privateChat.ClientSubscribe(
                        _ => _.ClanTagCharacterB,
                        _ => this.NotifyPropertyChanged(nameof(this.Title)),
                        this);
                }
                else
                {
                    privateChat.ClientSubscribe(
                        _ => _.IsClosedByCharacterB,
                        _ => this.RefreshTabVisibility(),
                        this);

                    privateChat.ClientSubscribe(
                        _ => _.ClanTagCharacterA,
                        _ => this.NotifyPropertyChanged(nameof(this.Title)),
                        this);
                }

                this.HasUnreadMessages = privateChat.ClientIsUnreadByCurrentCharacter();
            }
        }

        public BaseCommand CommandClosePrivateChat
            => new ActionCommand(this.ExecuteCommandClosePrivateChat);

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

        public bool IsPrivateChat => this.ChatRoom is ChatRoomPrivate;

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

        public bool IsTabVisible { get; private set; }

        public string Title => this.ChatRoom.ClientGetTitle();

        public void SetIsTabVisibleInPanel(bool isVisible)
        {
            this.isPanelAllowsTabVisibility = isVisible;
            this.RefreshTabVisibility();
        }

        public void TryResetUnreadFlag()
        {
            if (!this.HasUnreadMessages
                || !this.isSelected
                || !this.IsOpened)
            {
                return;
            }

            this.HasUnreadMessages = false;

            if (this.ChatRoom is ChatRoomPrivate privateChat)
                // this check is incorrect as this flag is no longer updated
                // (it's synchronized only once)
                //&& privateChat.ClientIsUnreadByCurrentCharacter())
            {
                ChatSystem.ClientSetPrivateChatRead(privateChat);
            }
        }

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

            if (this.IsSelected
                && Api.Client.Input.IsGameWindowFocused)
            {
                // don't set the "unread messages" indicator when message is coming to the currently selected tab
                // and the game window is focused
                if (this.ChatRoom is ChatRoomPrivate privateChat)
                {
                    ChatSystem.ClientSetPrivateChatRead(privateChat);
                }

                return;
            }

            this.HasUnreadMessages = true;
            this.IsTabVisible = this.HasUnreadMessages;
        }

        private void ExecuteCommandClosePrivateChat()
        {
            var chatRoom = (ChatRoomPrivate)this.ChatRoom;
            DialogWindow.ShowDialog(
                title: CoreStrings.QuestionAreYouSure,
                text: string.Format(DialogClosePrivateChat_Format,
                                    chatRoom.ClientGetTitle().TrimStart('@')),
                okText: CoreStrings.Yes,
                okAction: () =>
                          {
                              ChatSystem.ClientClosePrivateChat(chatRoom);
                              this.callbackPrivateChatRoomClosed?.Invoke(this);
                          },
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        private void RefreshTabVisibility()
        {
            var isTabVisible = this.isPanelAllowsTabVisibility;
            if (!isTabVisible)
            {
                this.IsTabVisible = false;
                return;
            }

            if (this.ChatRoom is ChatRoomPrivate privateChat)
            {
                isTabVisible = !privateChat.ClientIsClosedByCurrentCharacter();
            }

            this.IsTabVisible = isTabVisible;
        }
    }
}