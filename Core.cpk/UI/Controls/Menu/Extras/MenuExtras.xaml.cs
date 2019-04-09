namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Extras
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuExtras : BaseUserControl
    {
        private const double AutoScrollSpeed = 20;

        private const double MouseScrollWheelSpeed = 0.5;

        private bool isNeedToScrollToHome = true;

        private bool isSubscribed;

        private double? lastMouseHeldPosition;

        private int? pendingScrollWheelDelta;

        private ScrollViewer scrollViewer;

        protected override void InitControl()
        {
            this.scrollViewer = this.GetByName<ScrollViewer>("ScrollViewer");
            this.IsEnabledChanged += this.IsEnabledChangedHandler;
        }

        protected override void OnLoaded()
        {
            this.RefreshSubscriptions();

            this.scrollViewer.UpdateLayout();
            var scrollViewerHeight = this.scrollViewer.ViewportHeight;
            var content = (FrameworkElement)this.scrollViewer.Content;
            content.Margin = new Thickness(0, scrollViewerHeight, 0, scrollViewerHeight);
        }

        protected override void OnUnloaded()
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
            this.lastMouseHeldPosition = this.GetCurrentMousePosition();
        }

        private void MouseScrollWheelHandler(object sender, MouseWheelEventArgs e)
        {
            // scroll wheel event is handled by Update method
            e.Handled = true;
            this.pendingScrollWheelDelta = e.Delta;
        }

        private void RefreshSubscriptions()
        {
            if (this.IsEnabled)
            {
                if (this.isSubscribed)
                {
                    return;
                }

                this.isSubscribed = true;
                ClientComponentUpdateHelper.UpdateCallback += this.Update;
                this.PreviewMouseLeftButtonDown += this.MouseLeftButtonDownHandler;
                this.PreviewMouseWheel += this.MouseScrollWheelHandler;
                Api.Client.UI.LayoutRoot.PreviewMouseLeftButtonUp += this.GlobalMouseLeftButtonUpHandler;
                return;
            }

            if (!this.isSubscribed)
            {
                return;
            }

            ClientComponentUpdateHelper.UpdateCallback -= this.Update;
            this.PreviewMouseLeftButtonDown -= this.MouseLeftButtonDownHandler;
            this.PreviewMouseWheel -= this.MouseScrollWheelHandler;
            Api.Client.UI.LayoutRoot.PreviewMouseLeftButtonUp -= this.GlobalMouseLeftButtonUpHandler;
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
                verticalOffset += delta;
            }
            else
            {
                // mouse button is not held
                if (this.isNeedToScrollToHome)
                {
                    // requested to scroll to home
                    this.isNeedToScrollToHome = false;
                    this.scrollViewer.ScrollToHome();
                    return;
                }

                if (this.pendingScrollWheelDelta.HasValue)
                {
                    // requested to scroll by mouse scroll wheel delta
                    verticalOffset -= this.pendingScrollWheelDelta.Value * MouseScrollWheelSpeed;
                    this.pendingScrollWheelDelta = null;
                }
                else
                {
                    // auto scroll
                    verticalOffset += Api.Client.Core.DeltaTime * AutoScrollSpeed;
                }
            }

            if (verticalOffset >= this.scrollViewer.ScrollableHeight)
            {
                // scroll overlap to beginning
                verticalOffset = 0;
            }
            else if (verticalOffset < 0)
            {
                // scroll overlap to end
                verticalOffset = this.scrollViewer.ScrollableHeight - 1;
            }

            this.scrollViewer.ScrollToVerticalOffset(verticalOffset);
        }
    }
}