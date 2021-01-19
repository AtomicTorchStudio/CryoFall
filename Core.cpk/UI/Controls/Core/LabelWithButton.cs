namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class LabelWithButton : BaseContentControl
    {
        public static readonly DependencyProperty ButtonProperty =
            DependencyProperty.Register(
                nameof(Button),
                typeof(IButtonReference),
                typeof(LabelWithButton),
                new PropertyMetadata(default(IButtonReference), ButtonPropertyChangedCallback));

        public static readonly DependencyProperty TextKeyProperty =
            DependencyProperty.Register(
                nameof(TextKey),
                typeof(string),
                typeof(LabelWithButton),
                new PropertyMetadata(default(string)));

        static LabelWithButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LabelWithButton),
                new FrameworkPropertyMetadata(typeof(LabelWithButton)));
        }

        public IButtonReference Button
        {
            get => (IButtonReference)this.GetValue(ButtonProperty);
            set => this.SetValue(ButtonProperty, value);
        }

        public string TextKey
        {
            get => (string)this.GetValue(TextKeyProperty);
            set => this.SetValue(TextKeyProperty, value);
        }

        protected override void OnLoaded()
        {
            ClientInputManager.ButtonKeyMappingUpdated += this.ClientInputManagerButtonKeyMappingUpdatedHandler;
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            ClientInputManager.ButtonKeyMappingUpdated -= this.ClientInputManagerButtonKeyMappingUpdatedHandler;
        }

        private static void ButtonPropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = (LabelWithButton)dependencyObject;
            control.Refresh();
        }

        private void ClientInputManagerButtonKeyMappingUpdatedHandler(IWrappedButton obj)
        {
            if (obj == this.Button.AbstractButton)
            {
                this.Refresh();
            }
        }

        private void Refresh()
        {
            if (!this.isLoaded)
            {
                return;
            }

            var value = this.Button;
            this.TextKey = InputKeyNameHelper.GetKeyText(
                ClientInputManager.GetKeyForButton(value.AbstractButton));
        }
    }
}