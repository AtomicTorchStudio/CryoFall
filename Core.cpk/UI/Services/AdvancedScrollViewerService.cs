namespace AtomicTorch.CBND.CoreMod.UI.Services
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public static class AdvancedScrollViewerService
    {
        public static readonly DependencyProperty IsEnabledProperty
            = DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(AdvancedScrollViewerService),
                new PropertyMetadata(defaultValue: false, IsEnabledChangedHandler));

        private static ScrollViewer currentlyPressedScrollViewer;

        private static bool isAutoScrollToBottom;

        // this is used to detect whether the player is just scrolled the scroll viewer with the mouse wheel
        private static uint lastScrollViewerScrollWheelFrameNumber;

        private static ScrollViewer scrollViewerForAutoScrollToBottomCheck;

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)(element.GetValue(IsEnabledProperty) ?? false);
        }

        public static void ScrollToBottom(ScrollViewer scrollViewer, bool force)
        {
            if (scrollViewer == scrollViewerForAutoScrollToBottomCheck)
            {
                if (force)
                {
                    isAutoScrollToBottom = true;
                }
                else if (!isAutoScrollToBottom)
                {
                    return;
                }
            }

            scrollViewer.ScrollToBottom();
        }

        public static void SetIsEnabled(DependencyObject element, BaseCommand value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        private static void IsEnabledChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer scrollViewer)
            {
                Api.Logger.Error(d + " is not a ScrollViewer");
                return;
            }

            if (e.NewValue is not true)
            {
                Unregister(scrollViewer);
                return;
            }

            if (e.OldValue is true)
            {
                // already registered
                return;
            }

            Register(scrollViewer);
        }

        private static void Register(ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollChanged += ScrollViewerScrollChangedHandler;
            scrollViewer.PreviewMouseDown += ScrollViewerPreviewMouseDownOrUpHandler;
            scrollViewer.PreviewMouseUp += ScrollViewerPreviewMouseDownOrUpHandler;
            scrollViewer.PreviewMouseWheel += ScrollViewerMouseWheelHandler;
        }

        private static void ScrollViewerMouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            lastScrollViewerScrollWheelFrameNumber = Api.Client.Core.ClientFrameNumber;
        }

        private static void ScrollViewerPreviewMouseDownOrUpHandler(object sender, MouseButtonEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            switch (e.LeftButton)
            {
                case MouseButtonState.Pressed:
                    currentlyPressedScrollViewer = scrollViewer;
                    break;

                case MouseButtonState.Released:
                    if (currentlyPressedScrollViewer == scrollViewer)
                    {
                        currentlyPressedScrollViewer = null;
                    }

                    break;
            }
        }

        private static void ScrollViewerScrollChangedHandler(
            object sender,
            ScrollChangedEventArgs scrollChangedEventArgs)
        {
            var scrollViewer = (ScrollViewer)sender;
            var isUserScrollChange = scrollViewer == currentlyPressedScrollViewer
                                     || lastScrollViewerScrollWheelFrameNumber == Api.Client.Core.ClientFrameNumber;
            if (!isUserScrollChange)
            {
                return;
            }

            // when user scrolled the log
            var verticalOffset = scrollViewer.VerticalOffset;
            var scrollableHeight = scrollViewer.ScrollableHeight;
            isAutoScrollToBottom = verticalOffset == scrollableHeight;
            scrollViewerForAutoScrollToBottomCheck = scrollViewer;
        }

        private static void Unregister(ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollChanged -= ScrollViewerScrollChangedHandler;
            scrollViewer.PreviewMouseDown -= ScrollViewerPreviewMouseDownOrUpHandler;
            scrollViewer.PreviewMouseUp -= ScrollViewerPreviewMouseDownOrUpHandler;
            scrollViewer.PreviewMouseWheel -= ScrollViewerMouseWheelHandler;
        }
    }
}