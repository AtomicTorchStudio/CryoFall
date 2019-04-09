namespace AtomicTorch.CBND.CoreMod.UI.Services
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class ScrollViewerWithButtonsService
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ScrollViewerWithButtonsService),
            new PropertyMetadata(default(bool), PropertyChangedCallback));

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        private static void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            var child = (FrameworkElement)VisualTreeHelper.GetChild((FrameworkElement)sender, 0);
            child.DataContext = new ViewModelScrollViewerVerticalTemplate((ScrollViewer)sender);
        }

        private static void PropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                throw new Exception("You cannot disable this, only enable");
            }

            var frameworkElement = (FrameworkElement)dependencyObject;
            frameworkElement.Loaded += FrameworkElement_Loaded;
        }
    }
}