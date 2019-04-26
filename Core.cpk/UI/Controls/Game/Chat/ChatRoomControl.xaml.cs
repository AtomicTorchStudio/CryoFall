namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public partial class ChatRoomControl : BaseUserControl
    {
        public const double DelayOnChatCloseSeconds = 1.5;

        private const double DefaultChatHistoryInitialHideDelaySeconds = 8.0;

        private const double DefaultNewEntryHideDelaySeconds = 15.0;

        private const int MaxChatEntriesCount = 50;

        private static readonly SoundResource SoundResourceActivity
            = new SoundResource("UI/Chat/Activity");

        private static readonly SoundResource SoundResourceMessageReceived
            = new SoundResource("UI/Chat/Received");

        private static readonly SoundResource SoundResourceMessageSend
            = new SoundResource("UI/Chat/Send");

        private uint activatedOnFrameNumber;

        private bool? isActive;

        private bool isExpanded;

        private static uint lastMessageReceivedSoundPlayerFrameNumber;

        private ScrollViewer scrollViewerChatLog;

        private UIElementCollection stackPanelChatLogChildren;

        private TextBox textBoxChatInput;

        private ViewModelChatRoom viewModelChatRoom;

        public ChatPanel ChatPanel { get; set; }

        public bool IsActive
        {
            get => this.isActive ?? false;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;

                if (this.isActive.Value)
                {
                    this.activatedOnFrameNumber = Api.Client.CurrentGame.ServerFrameNumber;
                    this.IsExpanded = true;
                    this.ShowEntries();

                    this.textBoxChatInput.Visibility = Visibility.Visible;
                    this.textBoxChatInput.Focusable = true;

                    this.scrollViewerChatLog.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    this.scrollViewerChatLog.Focusable = true;

                    this.ScrollToEnd();

                    this.textBoxChatInput.KeyDown += this.InputKeyDownHandler;
                    this.textBoxChatInput.PreviewTextInput += this.InputPreviewTextInputHandler;
                    this.textBoxChatInput.PreviewKeyDown += this.InputPreviewKeyDownHandler;
                    this.scrollViewerChatLog.MouseLeftButtonUp += this.ScrollViewerChatLogMouseLeftButtonUpHandler;

                    this.viewModelChatRoom.IsOpened = true;

                    if (this.viewModelChatRoom.IsSelected)
                    {
                        this.FocusInput();
                    }
                }
                else
                {
                    // not active
                    this.IsExpanded = false;
                    this.textBoxChatInput.Visibility = Visibility.Hidden;
                    this.textBoxChatInput.Focusable = false;

                    this.HideEntries();

                    this.scrollViewerChatLog.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    this.scrollViewerChatLog.Focusable = false;

                    Api.Client.UI.BlurFocus();

                    this.textBoxChatInput.KeyDown -= this.InputKeyDownHandler;
                    this.textBoxChatInput.PreviewTextInput -= this.InputPreviewTextInputHandler;
                    this.textBoxChatInput.PreviewKeyDown -= this.InputPreviewKeyDownHandler;
                    this.scrollViewerChatLog.MouseLeftButtonUp -= this.ScrollViewerChatLogMouseLeftButtonUpHandler;

                    this.viewModelChatRoom.IsOpened = false;
                }
            }
        }

        public ViewModelChatRoom ViewModelChatRoom
        {
            get => this.viewModelChatRoom;
            set
            {
                if (this.viewModelChatRoom != null)
                {
                    throw new InvalidOperationException();
                }

                this.viewModelChatRoom = value;
                this.PopulateEntriesFromRoomLog();

                this.viewModelChatRoom.SubscribePropertyChange(
                    _ => _.IsSelected,
                    isSelected =>
                    {
                        if (isSelected && this.IsActive)
                        {
                            this.FocusInput();
                        }
                    });
                ;
            }
        }

        private bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                if (!this.isLoaded)
                {
                    return;
                }

                if (!value
                    && this.scrollViewerChatLog.IsFocused)
                {
                    value = true;
                }

                if (this.isExpanded == value)
                {
                    return;
                }

                this.isExpanded = value;

                if (this.isExpanded)
                {
                    this.scrollViewerChatLog.ClearValue(MaxHeightProperty);
                    this.ShowEntries();
                }
                else
                {
                    this.LimitScrollViewerHeight();
                    this.ScrollToEnd();
                    this.HideEntries();
                }

                this.IsHitTestVisible = this.IsExpanded;
                this.scrollViewerChatLog.IsHitTestVisible = this.IsExpanded;

                foreach (ChatEntryControl entry in this.stackPanelChatLogChildren)
                {
                    entry.IsHitTestVisible = this.IsExpanded;
                }
            }
        }

        public void AddMention(string from)
        {
            if (string.IsNullOrEmpty(from))
            {
                return;
            }

            this.IsActive = true;

            from = "@" + from + ", ";
            if (this.textBoxChatInput.Text.Length == 0)
            {
                this.textBoxChatInput.Text = from;
                this.textBoxChatInput.CaretIndex = from.Length;
            }
            else
            {
                var insertPosition = this.textBoxChatInput.CaretIndex;
                this.textBoxChatInput.Text = this.textBoxChatInput.Text.Insert(insertPosition, " " + from);
                this.textBoxChatInput.CaretIndex = insertPosition + 1 + from.Length;
            }
        }

        public void FocusInput()
        {
            // set focus on the next frame
            ClientComponentTimersManager.AddAction(
                0,
                () =>
                {
                    if (this.IsActive
                        && this.viewModelChatRoom.IsSelected)
                    {
                        this.textBoxChatInput.Focus();
                        Keyboard.Focus(this.textBoxChatInput);
                    }
                });
        }

        protected override void InitControl()
        {
            this.textBoxChatInput = this.GetByName<TextBox>("TextBoxChatInput");
            this.textBoxChatInput.Visibility = Visibility.Hidden;
            this.textBoxChatInput.Focusable = false;

            this.scrollViewerChatLog = this.GetByName<ScrollViewer>("ScrollViewerChatLog");
            this.stackPanelChatLogChildren = this.GetByName<StackPanel>("StackPanelChatLog").Children;

            this.LimitScrollViewerHeight();
        }

        protected override void OnLoaded()
        {
            ChatSystem.ClientChatRoomMessageReceived += this.ClientChatRoomMessageReceivedHandler;
            this.PopulateEntriesFromRoomLog();
        }

        protected override void OnUnloaded()
        {
            ChatSystem.ClientChatRoomMessageReceived -= this.ClientChatRoomMessageReceivedHandler;
        }

        private void AddChatEntry(ChatEntry chatEntry)
        {
            var chatEntryControl = this.CreateChatEntryControl(chatEntry);

            this.stackPanelChatLogChildren.Add(chatEntryControl);
            if (this.stackPanelChatLogChildren.Count > MaxChatEntriesCount)
            {
                this.stackPanelChatLogChildren.RemoveAt(0);
            }

            this.ScrollToEnd();

            if (this.isLoaded
                && !this.IsExpanded)
            {
                chatEntryControl.Hide(delaySeconds: DefaultNewEntryHideDelaySeconds);
            }
        }

        private void ClientChatRoomMessageReceivedHandler(BaseChatRoom chatRoom, in ChatEntry chatEntry)
        {
            if (this.viewModelChatRoom.ChatRoom != chatRoom)
            {
                // addressed to different chat room
                return;
            }

            this.AddChatEntry(chatEntry);
            this.ClientPlaySoundMessageReceived(chatEntry);
        }

        private void ClientPlaySoundMessageReceived(in ChatEntry chatEntry)
        {
            if (!this.ViewModelChatRoom.IsSelected)
            {
                return;
            }

            var frameNumber = Api.Client.CurrentGame.ServerFrameNumber;
            if (lastMessageReceivedSoundPlayerFrameNumber == frameNumber)
            {
                return;
            }

            lastMessageReceivedSoundPlayerFrameNumber = frameNumber;

            Api.Client.Audio.PlayOneShot(chatEntry.IsService
                                             ? SoundResourceActivity
                                             : SoundResourceMessageReceived,
                                         volume: SoundConstants.VolumeUIChat);
        }

        private ChatEntryControl CreateChatEntryControl(ChatEntry chatEntry)
        {
            var control = new ChatEntryControl();
            control.Setup(this, chatEntry);
            return control;
        }

        private void HideEntries()
        {
            if (!this.isLoaded
                || this.IsActive
                || this.IsExpanded)
            {
                return;
            }

            var count = this.stackPanelChatLogChildren.Count;

            for (var i = 0; i < count; i++)
            {
                var entry = (ChatEntryControl)this.stackPanelChatLogChildren[i];
                entry.Hide(delaySeconds: DelayOnChatCloseSeconds);
            }
        }

        private void InputKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (this.activatedOnFrameNumber == Api.Client.CurrentGame.ServerFrameNumber)
            {
                // ignore for now
                return;
            }

            this.ScrollToEnd();

            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    var message = this.textBoxChatInput.Text;
                    this.SendMessage(message);
                    this.textBoxChatInput.Text = string.Empty;

                    this.IsActive = false;
                    this.ChatPanel.Close();
                    break;

                case Key.Escape:
                    e.Handled = true;
                    this.IsActive = false;
                    this.ChatPanel.Close();
                    break;
            }
        }

        private void InputPreviewKeyDownHandler(object sender, KeyEventArgs e)
        {
            var isCtrlHeld = Api.Client.Input.IsKeyHeld(InputKey.Control, evenIfHandled: true);
            switch (e.Key)
            {
                case Key.Up:
                    if (!isCtrlHeld)
                    {
                        return;
                    }

                    e.Handled = true;
                    this.ChatPanel.SelectPreviousTab();
                    break;

                case Key.Down:
                    if (!isCtrlHeld)
                    {
                        return;
                    }

                    e.Handled = true;
                    this.ChatPanel.SelectNextTab();
                    break;

                case Key.Tab:
                    e.Handled = true;
                    if (!isCtrlHeld)
                    {
                        return;
                    }

                    if (Api.Client.Input.IsKeyHeld(InputKey.Shift, evenIfHandled: true))
                    {
                        this.ChatPanel.SelectPreviousTab();
                    }
                    else
                    {
                        this.ChatPanel.SelectNextTab();
                    }

                    break;
            }
        }

        private void InputPreviewTextInputHandler(object sender, TextCompositionEventArgs e)
        {
            if (this.textBoxChatInput.Text.Length != 0)
            {
                return;
            }

            switch (e.Text)
            {
                case "!":
                    e.Handled = true;
                    this.ChatPanel.SelectTab<ChatRoomLocal>();
                    break;

                case "$":
                    e.Handled = true;
                    this.ChatPanel.SelectTab<ChatRoomGlobal>();
                    break;

                case "#":
                    this.ChatPanel.SelectTab<ChatRoomParty>();
                    e.Handled = true;
                    break;
            }
        }

        private void LimitScrollViewerHeight()
        {
            this.scrollViewerChatLog.MaxHeight = 110;
        }

        private void PopulateEntriesFromRoomLog()
        {
            if (!this.isLoaded
                || this.viewModelChatRoom == null)
            {
                return;
            }

            this.stackPanelChatLogChildren.Clear();
            var chatEntries = ChatSystem.SharedGetChatRoom((ILogicObject)this.viewModelChatRoom.ChatRoom.GameObject)
                                        .ChatLog
                                        .ToList();

            if (chatEntries.Count == 0)
            {
                return;
            }

            chatEntries.Reverse();
            foreach (var chatEntry in chatEntries)
            {
                if (this.stackPanelChatLogChildren.Count > MaxChatEntriesCount)
                {
                    break;
                }

                var chatEntryControl = this.CreateChatEntryControl(chatEntry);
                this.stackPanelChatLogChildren.Insert(0, chatEntryControl);
                chatEntryControl.Hide(delaySeconds: DefaultChatHistoryInitialHideDelaySeconds);
            }

            this.ScrollToEnd();
        }

        private void ScrollToEnd()
        {
            this.scrollViewerChatLog.UpdateLayout();
            this.scrollViewerChatLog.ScrollToEnd();
        }

        private void ScrollViewerChatLogMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            this.FocusInput();
        }

        private void SendMessage(string message)
        {
            message = message.TrimNewLinesAndSpaces();
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var username = Api.Client.Characters.CurrentPlayerCharacter.Name;
            Api.Client.Audio
               .PlayOneShot(SoundResourceMessageSend,
                            volume: SoundConstants.VolumeUIChat);

            this.AddChatEntry(new ChatEntry(username,
                                            message,
                                            isService: false,
                                            DateTime.Now));

            ChatSystem.ClientSendMessageToRoom(this.ViewModelChatRoom.ChatRoom, message);
        }

        private void ShowEntries()
        {
            for (var i = 0; i < this.stackPanelChatLogChildren.Count; i++)
            {
                var entry = this.stackPanelChatLogChildren[i] as ChatEntryControl;
                entry?.Show();
            }
        }
    }
}