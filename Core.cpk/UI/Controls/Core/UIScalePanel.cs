namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    /// <summary>
    /// A special panel which should be used to apply the UI scale (which is an option in the General options menu).
    /// </summary>
    public class UIScalePanel : BaseContentControl
    {
        public static readonly DependencyProperty IsInverseProperty =
            DependencyProperty.Register(nameof(IsInverse),
                                        typeof(bool),
                                        typeof(UIScalePanel),
                                        new PropertyMetadata(default(bool)));

        private static readonly IUIClientService UIClientService = Api.IsClient ? Api.Client.UI : null;

        private ScaleTransform scaleTransform;

        static UIScalePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(UIScalePanel),
                new FrameworkPropertyMetadata(typeof(UIScalePanel)));
        }

        public bool IsInverse
        {
            get => (bool)this.GetValue(IsInverseProperty);
            set => this.SetValue(IsInverseProperty, value);
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

            UIClientService.ScaleChanged += this.UIScaleChangedHandler;
        }

        protected override void OnUnloaded()
        {
            UIClientService.ScaleChanged -= this.UIScaleChangedHandler;
        }

        private void RefreshScale()
        {
            var scale = UIClientService.Scale;

            if (this.IsInverse)
            {
                scale = 1 / scale;
            }

            this.scaleTransform.ScaleX = this.scaleTransform.ScaleY = scale;
        }

        private void UIScaleChangedHandler()
        {
            this.RefreshScale();
        }
    }
}