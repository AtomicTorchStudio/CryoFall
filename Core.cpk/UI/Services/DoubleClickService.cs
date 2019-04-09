namespace AtomicTorch.CBND.CoreMod.UI.Services
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public static class DoubleClickService
    {
        public const double DoubleClickMaxDurationSeconds = 0.3;

        public static readonly DependencyProperty CommandOnDoubleClickProperty
            = DependencyProperty.RegisterAttached(
                "CommandOnDoubleClick",
                typeof(BaseCommand),
                typeof(DoubleClickService),
                new PropertyMetadata(null, CommandOnDoubleClickChangedHandler));

        public static readonly DependencyProperty CommandOnDoubleClickParameterProperty
            = DependencyProperty.RegisterAttached(
                "CommandOnDoubleClickParameter",
                typeof(object),
                typeof(DoubleClickService),
                new PropertyMetadata(default(object)));

        private static readonly List<ElementLastClickTime> RecentlyClickedList
            = new List<ElementLastClickTime>();

        public static BaseCommand GetCommandOnDoubleClick(DependencyObject element)
        {
            return (BaseCommand)element.GetValue(CommandOnDoubleClickProperty);
        }

        public static object GetCommandOnDoubleClickParameter(DependencyObject element)
        {
            return element.GetValue(CommandOnDoubleClickParameterProperty);
        }

        public static void SetCommandOnDoubleClick(DependencyObject element, BaseCommand value)
        {
            element.SetValue(CommandOnDoubleClickProperty, value);
        }

        public static void SetCommandOnDoubleClickParameter(DependencyObject element, object value)
        {
            element.SetValue(CommandOnDoubleClickParameterProperty, value);
        }

        private static void CommandOnDoubleClickChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)d;
            if (e.NewValue == null)
            {
                Unregister(frameworkElement);
                return;
            }

            if (e.OldValue != null)
            {
                // already registered
                return;
            }

            Register(frameworkElement);
        }

        private static void FrameworkElementMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            var currentTime = Api.Client.Core.ClientRealTime;
            var frameworkElement = (FrameworkElement)sender;
            for (var index = 0; index < RecentlyClickedList.Count; index++)
            {
                var pair = RecentlyClickedList[index];
                var otherElement = pair.FrameworkElement;
                var timeElapsedSeconds = currentTime - pair.LastClickTime;
                if (!ReferenceEquals(otherElement, frameworkElement))
                {
                    // different control
                    if (timeElapsedSeconds > DoubleClickMaxDurationSeconds)
                    {
                        // unregister control from double click handling
                        RecentlyClickedList.RemoveAt(index);
                        index--;
                    }

                    continue;
                }

                // the same element clicked - unregister it
                RecentlyClickedList.RemoveAt(index);

                // check if this is a double click
                if (timeElapsedSeconds <= DoubleClickMaxDurationSeconds)
                {
                    // double click!
                    var command = GetCommandOnDoubleClick(frameworkElement);
                    if (command != null)
                    {
                        // invoke attached command
                        var parameter = GetCommandOnDoubleClickParameter(frameworkElement);
                        command.Execute(parameter);
                    }
                }
                else
                {
                    // not a double click - register single click - remember click time
                    RecentlyClickedList.Add(new ElementLastClickTime(frameworkElement, currentTime));
                }

                // stop processing of the list
                return;
            }

            // element not found - this means this is a first click on it - remember click time
            RecentlyClickedList.Add(new ElementLastClickTime(frameworkElement, currentTime));
        }

        private static void Register(FrameworkElement frameworkElement)
        {
            // there are no memory leak nor preventing of GC for frameworkElement - FrameworkElementPreviewKeyUpHandler is static method
            frameworkElement.MouseLeftButtonUp += FrameworkElementMouseLeftButtonUpHandler;
        }

        private static void Unregister(FrameworkElement frameworkElement)
        {
            // there are no memory leak nor preventing of GC for frameworkElement - FrameworkElementPreviewKeyUpHandler is static method
            frameworkElement.MouseLeftButtonUp -= FrameworkElementMouseLeftButtonUpHandler;
        }

        private struct ElementLastClickTime
        {
            public readonly FrameworkElement FrameworkElement;

            public readonly double LastClickTime;

            public ElementLastClickTime(FrameworkElement frameworkElement, double lastClickTime)
            {
                this.FrameworkElement = frameworkElement;
                this.LastClickTime = lastClickTime;
            }
        }
    }
}