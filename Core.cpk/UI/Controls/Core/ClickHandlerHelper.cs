namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClickHandlerHelper : IDisposable
    {
        private const double ClickMouseDistancePixels = 80;

        private const double ClickTime = 0.25;

        private static readonly ICoreClientService Core = Api.Client.Core;

        private static readonly Grid GlobalLayoutRoot = Api.Client.UI.LayoutRoot;

        public Action<MouseEventArgs> OnEvent;

        private FrameworkElement control;

        private Vector2D mouseLeftButtonDownPosition;

        private double mouseLeftButtonDownTime;

        public ClickHandlerHelper(FrameworkElement control, Action<MouseEventArgs> onEvent)
        {
            this.control = control;
            this.OnEvent = onEvent;
            control.MouseLeftButtonDown += this.MouseLeftButtonDownHandler;
            control.MouseLeftButtonUp += this.MouseRightButtonUpHandler;
        }

        public void Dispose()
        {
            this.control.MouseLeftButtonDown -= this.MouseLeftButtonDownHandler;
            this.control.MouseLeftButtonUp -= this.MouseRightButtonUpHandler;
            this.control = null;
            this.OnEvent = null;
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            this.mouseLeftButtonDownTime = Core.ClientRealTime;
            this.mouseLeftButtonDownPosition = e.GetPosition(GlobalLayoutRoot).ToVector2D();
        }

        private void MouseRightButtonUpHandler(object o, MouseButtonEventArgs e)
        {
            var newPos = e.GetPosition(GlobalLayoutRoot).ToVector2D();

            var time = Core.ClientRealTime;
            if (time - this.mouseLeftButtonDownTime <= ClickTime
                && (this.mouseLeftButtonDownPosition - newPos).Length <= ClickMouseDistancePixels)
            {
                this.OnEvent(e);
            }
        }
    }
}