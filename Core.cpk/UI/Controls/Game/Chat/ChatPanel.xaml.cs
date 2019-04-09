namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ChatPanel : BaseUserControl
    {
        private readonly Dictionary<BaseChatRoom, ChatRoomTab> chatRooms
            = new Dictionary<BaseChatRoom, ChatRoomTab>();

        private bool? isActive;

        private ClientInputContext openedChatInputContext;

        private TabControlCached tabControl;

        public static ChatPanel Instance { get; private set; }

        public bool IsActive
        {
            get => this.isActive ?? false;
            private set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;
                this.Background = this.isActive.Value
                                      ? Brushes.Transparent
                                      : null;

                this.IsHitTestVisible
                    = this.tabControl.IsHitTestVisible
                          = this.isActive.Value;

                if (this.isActive.Value)
                {
                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    this.openedChatInputContext =
                        ClientInputContext.Start("Chat panel - intercept all other game input")
                                          .HandleAll(
                                              () =>
                                              {
                                                  if (ClientInputManager.IsButtonDown(
                                                      GameButton.CancelOrClose,
                                                      evenIfHandled: true))
                                                  {
                                                      this.IsActive = false;
                                                  }

                                                  ClientInputManager.ConsumeAllButtons();
                                              });

                    foreach (var chatRoomTab in this.chatRooms.Values)
                    {
                        chatRoomTab.ViewModelChatRoom.IsTabVisible = true;
                        chatRoomTab.ChatRoomControl.IsActive = true;
                    }
                }
                else
                {
                    this.openedChatInputContext?.Stop();
                    this.openedChatInputContext = null;

                    foreach (var chatRoomTab in this.chatRooms.Values)
                    {
                        chatRoomTab.ViewModelChatRoom.IsTabVisible = false;
                        chatRoomTab.ChatRoomControl.IsActive = false;
                    }
                }
            }
        }

        public void Close()
        {
            this.IsActive = false;
        }

        public void Open()
        {
            this.IsActive = true;
        }

        public void OpenChat(BaseChatRoom chatRoom)
        {
            if (!this.chatRooms.TryGetValue(chatRoom, out var chatRoomTab))
            {
                return;
            }

            this.IsActive = true;
            chatRoomTab.TabItem.IsSelected = true;
        }

        public void SelectNextTab()
        {
            var index = this.tabControl.SelectedIndex + 1;
            var maxIndex = this.tabControl.Items.Count;
            if (index >= maxIndex)
            {
                index = 0;
            }

            this.tabControl.SelectedIndex = index;
        }

        public void SelectPreviousTab()
        {
            var index = this.tabControl.SelectedIndex - 1;
            if (index < 0)
            {
                index = this.tabControl.Items.Count - 1;
            }

            this.tabControl.SelectedIndex = index;
        }

        public void SelectTab<TChatRoom>()
            where TChatRoom : BaseChatRoom
        {
            foreach (var chatRoomTab in this.chatRooms.Values)
            {
                if (chatRoomTab.ChatRoom is TChatRoom)
                {
                    this.tabControl.SelectedItem = chatRoomTab.TabItem;
                }
            }
        }

        protected override void InitControl()
        {
            this.tabControl = this.GetByName<TabControlCached>("TabControl");
            this.MouseLeftButtonDown += this.MouseLeftButtonDownHandler;
            this.IsActive = false;
            Instance = this;

            ChatSystem.ClientChatRoomAdded += this.ChatRoomAddedHandler;
            ChatSystem.ClientChatRoomRemoved += this.ChatRoomRemovedHandler;
            this.tabControl.MouseUp += this.TabControlMouseUp;
        }

        private static int CompareTabs(TabItem tabA, TabItem tabB)
        {
            var chatRoomA = (ViewModelChatRoom)tabA.DataContext;
            var chatRoomB = (ViewModelChatRoom)tabB.DataContext;
            var orderA = GetTabOrder(chatRoomA);
            var orderB = GetTabOrder(chatRoomB);
            if (orderA != orderB)
            {
                return orderB.CompareTo(orderA);
            }

            int GetTabOrder(ViewModelChatRoom viewModelChatRoom)
            {
                switch (viewModelChatRoom.ChatRoom)
                {
                    case ChatRoomGlobal _:
                        return 0;

                    case ChatRoomLocal _:
                        return 1;

                    case ChatRoomParty _:
                        return 2;

                    default:
                        return int.MaxValue;
                }
            }

            var lastMessageA = chatRoomA.ChatRoom.ChatLog.LastOrDefault();
            var lastMessageB = chatRoomB.ChatRoom.ChatLog.LastOrDefault();
            return lastMessageB.UtcDate.CompareTo(lastMessageA.UtcDate);
        }

        private void ChatRoomAddedHandler(BaseChatRoom chatRoom)
        {
            if (chatRoom is ChatRoomPrivate privateChatRoom)
            {
                if (ClientChatBlockList.IsBlocked(privateChatRoom.CharacterA)
                    || ClientChatBlockList.IsBlocked(privateChatRoom.CharacterB))
                {
                    // chat room with blocked player - ignore it
                    return;
                }
            }

            var viewModelChatRoom = new ViewModelChatRoom(chatRoom,
                                                          callbackNeedTabSort:
                                                          () => this.tabControl.SortTabs(CompareTabs));
            var chatRoomControl = new ChatRoomControl()
            {
                ViewModelChatRoom = viewModelChatRoom,
                ChatPanel = this
            };

            var tabItem = new TabItem()
            {
                Header = chatRoom.ClientGetTitle(),
                Content = chatRoomControl,
                DataContext = viewModelChatRoom
            };

            this.chatRooms.Add(chatRoom,
                               new ChatRoomTab(chatRoom, chatRoomControl, tabItem, viewModelChatRoom));

            this.tabControl.AddItem(tabItem);

            if (this.IsActive)
            {
                viewModelChatRoom.IsTabVisible = true;
                chatRoomControl.IsActive = true;
            }

            if (chatRoom is ChatRoomGlobal)
            {
                tabItem.IsSelected = true;
            }

            this.tabControl.SortTabs(CompareTabs);
        }

        private void ChatRoomRemovedHandler(BaseChatRoom chatRoom)
        {
            if (!this.chatRooms.TryGetValue(chatRoom, out var chatRoomTab))
            {
                return;
            }

            this.chatRooms.Remove(chatRoom);
            this.tabControl.Remove(chatRoomTab.TabItem);

            chatRoomTab.TabItem.DataContext = null;
            chatRoomTab.ViewModelChatRoom.Dispose();
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (this.tabControl.IsMouseOver)
            {
                // ignore
                return;
            }

            // close chats
            this.IsActive = false;
        }

        private void TabControlMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsActive)
            {
                return;
            }

            // focus on current chat room control
            var chatRoomControl = (ChatRoomControl)((TabItem)this.tabControl.SelectedItem).Content;
            chatRoomControl.FocusInput();
        }

        private class ChatRoomTab
        {
            public ChatRoomTab(
                BaseChatRoom chatRoom,
                ChatRoomControl chatRoomControl,
                TabItem tabItem,
                ViewModelChatRoom viewModelChatRoom)
            {
                this.ChatRoom = chatRoom;
                this.ChatRoomControl = chatRoomControl;
                this.TabItem = tabItem;
                this.ViewModelChatRoom = viewModelChatRoom;
            }

            public BaseChatRoom ChatRoom { get; }

            public ChatRoomControl ChatRoomControl { get; }

            public TabItem TabItem { get; }

            public ViewModelChatRoom ViewModelChatRoom { get; }
        }
    }
}