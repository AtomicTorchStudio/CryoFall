namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    /// <summary>
    /// A special panel which should be used only inside the popups to ensure that the screen scale is applied properly.
    /// Please note that you might also need to use <see cref="UIScalePanel" /> inside this panel
    /// to apply the UI scale as well.
    /// </summary>
    public class ScreenScalePanel : BaseContentControl
    {
        private ScaleTransform scaleTransform;

        static ScreenScalePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ScreenScalePanel),
                new FrameworkPropertyMetadata(typeof(ScreenScalePanel)));
        }

        protected override void InitControl()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            var layoutRoot = templateRoot.GetByName<ContentControl>("LayoutRoot");
            layoutRoot.LayoutTransform = this.scaleTransform = new ScaleTransform();
            this.RefreshScale();
        }

        protected override void OnLoaded()
        {
            this.RefreshScale();
        }

        private void RefreshScale()
        {
            var ui = Api.Client.UI;
            var scale = ui.GetScreenScaleCoefficient();

            this.scaleTransform.ScaleX = this.scaleTransform.ScaleY = scale;
        }
    }
}