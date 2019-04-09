namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class RootViewbox : BaseContentControl
    {
        private static readonly IUIClientService UI = Api.IsClient ? Api.Client.UI : null;

        private ContentPresenter contentPresenter;

        private ScaleTransform contentPresenterScaleTransform;

        static RootViewbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RootViewbox),
                new FrameworkPropertyMetadata(typeof(RootViewbox)));
        }

        public RootViewbox()
        {
        }

        public void RefreshSize()
        {
            this.Refresh();
        }

        protected override void InitControl()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            // force stretch horizontal/vertical alignment
            templateRoot.HorizontalAlignment = HorizontalAlignment.Stretch;
            templateRoot.VerticalAlignment = VerticalAlignment.Stretch;

            this.contentPresenter = templateRoot.GetByName<ContentPresenter>("ContentPresenter");
            this.contentPresenterScaleTransform = new ScaleTransform();
            this.contentPresenter.RenderTransform = this.contentPresenterScaleTransform;
            this.Refresh();
        }

        protected override void OnLoaded()
        {
            UI.ScreenSizeChanged += this.ScreenSizeChangedHandler;
            UI.ScaleChanged += this.UIScaleChangedHandler;
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            UI.ScreenSizeChanged -= this.ScreenSizeChangedHandler;
            UI.ScaleChanged -= this.UIScaleChangedHandler;
        }

        private void Refresh()
        {
            if (!this.isLoaded)
            {
                return;
            }

            var screenWidth = UI.ScreenWidth;
            var screenHeight = UI.ScreenHeight;
            if (screenWidth == 0
                || screenHeight == 0)
            {
                // not yet ready
                return;
            }

            var targetWidth = UI.TargetUIWidth;
            var targetHeight = UI.TargetUIHeight;

            var scale = UI.GetScreenScaleCoefficient();

            targetWidth += (screenWidth - targetWidth * scale) / scale;
            targetHeight += (screenHeight - targetHeight * scale) / scale;

            this.contentPresenter.Width = targetWidth;
            this.contentPresenter.Height = targetHeight;

            this.contentPresenterScaleTransform.ScaleX = (float)scale;
            this.contentPresenterScaleTransform.ScaleY = (float)scale;
        }

        private void ScreenSizeChangedHandler()
        {
            this.Refresh();
        }

        private void UIScaleChangedHandler()
        {
            this.Refresh();
        }
    }
}