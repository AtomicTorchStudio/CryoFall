namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System.Windows;

    public static class VisualStateHelper
    {
        public static readonly DependencyProperty StateNameProperty = DependencyProperty.RegisterAttached(
            "StateName",
            typeof(string),
            typeof(VisualStateHelper),
            new UIPropertyMetadata(null, StateNameChanged));

        public static string GetStateName(DependencyObject obj)
        {
            return obj.GetValue(StateNameProperty) as string;
        }

        public static void SetStateName(DependencyObject obj, string value)
        {
            obj.SetValue(StateNameProperty, value);
        }

        internal static void StateNameChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is string stateName)
            {
                VisualStateManager.GoToElementState(
                    (FrameworkElement)target,
                    stateName,
                    useTransitions: false);
            }
        }
    }
}