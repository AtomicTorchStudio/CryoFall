namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Extras
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class AutoScrollViewer
    {
        private const double MouseScrollWheelSpeed = 0.5;

        private readonly double autoScrollSpeed;

        private readonly ScrollViewer scrollViewer;

        private bool isNeedToScrollToHome = true;

        private bool isSubscribed;

        private double? lastMouseHeldPosition;

        private int? pendingScrollWheelDelta;

        public AutoScrollViewer(ScrollViewer scrollViewer, bool isLoopedScroll, double autoScrollSpeed = 40)
        {
            this.autoScrollSpeed = autoScrollSpeed;
            this.scrollViewer = scrollViewer;
            this.IsLoopedScroll = isLoopedScroll;

            scrollViewer.IsEnabledChanged += this.IsEnabledChangedHandler;
            scrollViewer.Loaded += this.OnLoaded;
            scrollViewer.Unloaded += this.OnUnloaded;
        }

        public event Action Finished;

        public bool IsInputAllowed { get; set; } = true;

        public bool IsLoopedScroll { get; set; }

        public double SpeedMultiplier { get; set; } = 1.0;

        protected void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.RefreshSubscriptions();

            this.scrollViewer.UpdateLayout();
            var viewportHeight = this.scrollViewer.ViewportHeight;

            var content = (FrameworkElement)this.scrollViewer.Content;
            content.Margin = new Thickness(0, viewportHeight, 0, viewportHeight);
        }

        protected void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.RefreshSubscriptions();
        }

        private double GetCurrentMousePosition()
        {
            var pos = Api.Client.Input.MouseScreenPosition;
            return this.scrollViewer
                       .PointFromScreen(new Point(pos.X, pos.Y))
                       .Y;
        }

        private void GlobalMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            this.lastMouseHeldPosition = null;
        }

        private void IsEnabledChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.isNeedToScrollToHome = true;
            this.RefreshSubscriptions();
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (this.IsInputAllowed)
            {
                this.lastMouseHeldPosition = this.GetCurrentMousePosition();
            }
        }

        private void MouseScrollWheelHandler(object sender, MouseWheelEventArgs e)
        {
            // scroll wheel event is handled by Update method
            e.Handled = true;
            this.pendingScrollWheelDelta = e.Delta;
        }

        private void RefreshSubscriptions()
        {
            var ui = Api.Client.UI;
            if (!ui.IsReady)
            {
                // the UI/scripts are reloading now
                return;
            }

            if (this.scrollViewer.IsEnabled)
            {
                if (this.isSubscribed)
                {
                    return;
                }

                this.isSubscribed = true;
                ClientUpdateHelper.UpdateCallback += this.Update;
                this.scrollViewer.PreviewMouseLeftButtonDown += this.MouseLeftButtonDownHandler;
                this.scrollViewer.PreviewMouseWheel += this.MouseScrollWheelHandler;
                ui.LayoutRoot.PreviewMouseLeftButtonUp += this.GlobalMouseLeftButtonUpHandler;
                return;
            }

            if (!this.isSubscribed)
            {
                return;
            }

            ClientUpdateHelper.UpdateCallback -= this.Update;
            this.scrollViewer.PreviewMouseLeftButtonDown -= this.MouseLeftButtonDownHandler;
            this.scrollViewer.PreviewMouseWheel -= this.MouseScrollWheelHandler;
            ui.LayoutRoot.PreviewMouseLeftButtonUp -= this.GlobalMouseLeftButtonUpHandler;
            this.isSubscribed = false;
        }

        private void Update()
        {
            var verticalOffset = this.scrollViewer.VerticalOffset;
            if (this.lastMouseHeldPosition.HasValue)
            {
                // mouse button is held
                var position = this.GetCurrentMousePosition();
                // calculate and use the mouse position delta
                var delta = this.lastMouseHeldPosition.Value - position;
                this.lastMouseHeldPosition = position;

                if (this.IsInputAllowed)
                {
                    verticalOffset += delta;
                }
            }
            else
            {
                // mouse button is not held
                if (this.isNeedToScrollToHome)
                {
                    // requested to scroll to home
                    this.isNeedToScrollToHome = false;
                    var firstElement = this.scrollViewer.FindName<FrameworkElement>("GameLogo");

                    if (firstElement is null)
                    {
                        firstElement = (FrameworkElement)this.scrollViewer.Content;
                    }

                    // offset scroll viewer vertical position so it will display the logo in the center
                    firstElement.UpdateLayout();
                    var verticalPosition = firstElement.Margin.Bottom + firstElement.ActualHeight;

                    this.scrollViewer.ScrollToVerticalOffset(verticalPosition);
                    return;
                }

                if (this.pendingScrollWheelDelta.HasValue
                    && this.IsInputAllowed)
                {
                    // requested to scroll by mouse scroll wheel delta
                    verticalOffset -= this.pendingScrollWheelDelta.Value * MouseScrollWheelSpeed;
                    this.pendingScrollWheelDelta = null;
                }
                else
                {
                    // auto scroll
                    verticalOffset += Api.Client.Core.DeltaTime * this.autoScrollSpeed * this.SpeedMultiplier;
                }
            }

            if (verticalOffset >= this.scrollViewer.ScrollableHeight)
            {
                if (this.IsLoopedScroll)
                {
                    // scroll overlap to beginning
                    verticalOffset = 0;
                }
                else
                {
                    verticalOffset = this.scrollViewer.ScrollableHeight;
                }

                this.Finished?.Invoke();
            }
            else if (verticalOffset < 0)
            {
                if (this.IsLoopedScroll)
                {
                    // scroll overlap to end
                    verticalOffset = this.scrollViewer.ScrollableHeight - 1;
                }
                else
                {
                    verticalOffset = 0;
                }
            }

            this.scrollViewer.ScrollToVerticalOffset(verticalOffset);
        }
    }
}