namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.Systems.EmojiSystem;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class EmojiPickerPopupControl : BaseUserControl
    {
        private readonly ChatRoomControl chatRoomControl;

        private readonly FrameworkElement placementTarget;

        private Popup popup;

        public EmojiPickerPopupControl(ChatRoomControl chatRoomControl, FrameworkElement placementTarget)
        {
            this.chatRoomControl = chatRoomControl;
            this.placementTarget = placementTarget;
        }

        public EmojiPickerPopupControl()
        {
        }

        protected override void InitControl()
        {
            var emojiButtonStyle = this.GetResource<Style>("EmojiButtonStyle");
            var gridEmojis = this.GetByName<Panel>("GridEmojis");
            var children = gridEmojis.Children;

            var commandSelectEmoji = new ActionCommandWithParameter(this.ExecuteCommandSelectEmoji);

            foreach (var emojiData in EmojiSystem.EmojiCategories[0].Emojis)
            {
                children.Add(new Button()
                {
                    Command = commandSelectEmoji,
                    CommandParameter = emojiData.UnicodeId,
                    Content = emojiData.UnicodeId,
                    Style = emojiButtonStyle
                });
            }

            this.popup = this.GetByName<Popup>("PopupControl");
            this.popup.IsOpen = true;
            this.popup.SetValue(Popup.PlacementTargetProperty, this.placementTarget);
        }

        protected override void OnLoaded()
        {
            Api.Client.UI.LayoutRoot.PreviewMouseDown += this.GlobalRootPreviewMouseDownHandler;
            ClientUpdateHelper.UpdateCallback += this.Update;
        }

        protected override void OnUnloaded()
        {
            Api.Client.UI.LayoutRoot.PreviewMouseDown -= this.GlobalRootPreviewMouseDownHandler;
            ClientUpdateHelper.UpdateCallback -= this.Update;
        }

        private void ExecuteCommandSelectEmoji(object obj)
        {
            var emoji = (string)obj;
            this.chatRoomControl.OnEmojiSelected(emoji);
            this.Hide();
        }

        private void GlobalRootPreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (this.popup.IsMouseOver)
            {
                // don't handle clicks over the popup and its content
                return;
            }

            e.Handled = true;
            this.Hide();
        }

        private void Hide()
        {
            Api.Client.UI.LayoutRootChildren.Remove(this);
        }

        private void Update()
        {
            if (!this.chatRoomControl.IsActive)
            {
                this.Hide();
            }
        }
    }
}