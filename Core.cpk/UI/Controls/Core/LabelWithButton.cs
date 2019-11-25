namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi.Scripting;
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

        private static void ButtonPropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = (LabelWithButton)dependencyObject;
            var value = dependencyPropertyChangedEventArgs.NewValue;
            if (!(value is IButtonReference buttonReference))
            {
                Api.Logger.Error(
                    $"You need to provide a valid ButtonReference for {nameof(LabelWithButton)} control."
                    + $"{Environment.NewLine}Currently provided value is {value}");
                return;
            }

            control.UpdateTextKey(buttonReference.AbstractButton);
        }

        private void UpdateTextKey(IWrappedButton button)
        {
            this.TextKey = ClientInputManager.GetKeyForAbstractButton(button)
                                             .ToString();
        }
    }
}