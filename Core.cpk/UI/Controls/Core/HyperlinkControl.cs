namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class HyperlinkControl : BaseContentControl
    {
        public static readonly DependencyProperty ForegroundMouseOverProperty =
            DependencyProperty.Register(
                nameof(ForegroundMouseOver),
                typeof(Brush),
                typeof(HyperlinkControl),
                new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register(
                nameof(Url),
                typeof(string),
                typeof(HyperlinkControl),
                new PropertyMetadata(default(string)));

        private ContentPresenter contentPresenter;

        static HyperlinkControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HyperlinkControl),
                new FrameworkPropertyMetadata(typeof(HyperlinkControl)));
        }

        public Brush ForegroundMouseOver
        {
            get => (Brush)this.GetValue(ForegroundMouseOverProperty);
            set => this.SetValue(ForegroundMouseOverProperty, value);
        }

        public string Url
        {
            get => (string)this.GetValue(UrlProperty);
            set => this.SetValue(UrlProperty, value);
        }

        protected override void InitControl()
        {
            var root = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            this.contentPresenter = root.GetByName<ContentPresenter>("ContentPresenter");

            // remap content to internal content presenter (that's a hack!)
            var content = this.Content;
            this.Content = null;
            this.contentPresenter.Content = content;
        }

        protected override void OnLoaded()
        {
            this.MouseLeftButtonDown += this.MouseLeftButtonUpHandler;
        }

        protected override void OnUnloaded()
        {
            this.MouseLeftButtonDown -= this.MouseLeftButtonUpHandler;
        }

        private void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Api.Client.Core.OpenWebPage(this.Url);
        }
    }
}