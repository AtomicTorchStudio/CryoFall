namespace AtomicTorch.CBND.CoreMod.UI.Services
{
    using System.Windows;
    using System.Windows.Input;

    public static class FocusService
    {
        public static readonly DependencyProperty MoveFocusOnEnterKeyProperty = DependencyProperty.RegisterAttached(
            "MoveFocusOnEnterKey",
            typeof(bool),
            typeof(FocusService),
            new FrameworkPropertyMetadata(false, MoveFocusOnEnterKeyChangedHandler));

        public static bool GetMoveFocusOnEnterKey(FrameworkElement element)
        {
            return (bool)element.GetValue(MoveFocusOnEnterKeyProperty);
        }

        public static void Register(FrameworkElement frameworkElement)
        {
            // there are no memory leak nor preventing of GC for frameworkElement - FrameworkElementPreviewKeyUpHandler is static method
            frameworkElement.PreviewKeyUp += FrameworkElementPreviewKeyUpHandler;
        }

        public static void SetMoveFocusOnEnterKey(FrameworkElement element, bool value)
        {
            element.SetValue(MoveFocusOnEnterKeyProperty, value);
        }

        private static void FrameworkElementPreviewKeyUpHandler(object sender, KeyEventArgs args)
        {
            if (args.Key != Key.Enter)
            {
                return;
            }

            var frameworkElement = (FrameworkElement)sender;
            var moveFocusOnEnterKey = (bool)frameworkElement.GetValue(MoveFocusOnEnterKeyProperty);
            if (!moveFocusOnEnterKey)
            {
                return;
            }

            var focusElement = frameworkElement.PredictFocus(FocusNavigationDirection.Down);
            (focusElement as FrameworkElement)?.Focus();
        }

        private static void MoveFocusOnEnterKeyChangedHandler(
            DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            Register((FrameworkElement)obj);
        }
    }
}