namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WidgetPanel : BaseUserControl
    {
        public static readonly DependencyProperty InnerGlowSizeProperty =
            DependencyProperty.Register("InnerGlowSize",
                                        typeof(double),
                                        typeof(WidgetPanel),
                                        new PropertyMetadata(20.0));

        public static readonly DependencyProperty VisibilityBackgroundImageProperty =
            DependencyProperty.Register("VisibilityBackgroundImage",
                                        typeof(Visibility),
                                        typeof(WidgetPanel),
                                        new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty WidgetBackgroundProperty =
            DependencyProperty.Register("WidgetBackground",
                                        typeof(Brush),
                                        typeof(WidgetPanel),
                                        new PropertyMetadata(default(Brush)));

        public WidgetPanel()
        {
        }

        public double InnerGlowSize
        {
            get => (double)this.GetValue(InnerGlowSizeProperty);
            set => this.SetValue(InnerGlowSizeProperty, value);
        }

        public Visibility VisibilityBackgroundImage
        {
            get => (Visibility)this.GetValue(VisibilityBackgroundImageProperty);
            set => this.SetValue(VisibilityBackgroundImageProperty, value);
        }

        public Brush WidgetBackground
        {
            get => (Brush)this.GetValue(WidgetBackgroundProperty);
            set => this.SetValue(WidgetBackgroundProperty, value);
        }

        protected override void InitControl()
        {
        }
    }
}