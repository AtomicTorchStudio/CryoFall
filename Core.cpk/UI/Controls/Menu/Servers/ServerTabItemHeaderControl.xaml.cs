namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ServerTabItemHeaderControl : BaseUserControl
    {
        public static readonly DependencyProperty IconProperty
            = DependencyProperty.Register(
                nameof(Icon),
                typeof(Brush),
                typeof(ServerTabItemHeaderControl),
                new PropertyMetadata(Brushes.DarkOrange));

        public static readonly DependencyProperty ServersCountProperty
            = DependencyProperty.Register(
                nameof(ServersCount),
                typeof(ushort),
                typeof(ServerTabItemHeaderControl),
                new PropertyMetadata(defaultValue: (ushort)1000));

        public static readonly DependencyProperty IconImageSourceProperty
            = DependencyProperty.Register(
                nameof(IconImageSource),
                typeof(ImageSource),
                typeof(ServerTabItemHeaderControl),
                new PropertyMetadata(default(ImageSource), IconBrushImageSourceChangedHandler));

        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(ServerTabItemHeaderControl),
                new PropertyMetadata("Text"));

        public static readonly DependencyProperty VisibilityDetailsProperty
            = DependencyProperty.Register(
                nameof(VisibilityDetails),
                typeof(Visibility),
                typeof(ServerTabItemHeaderControl),
                new PropertyMetadata(Visibility.Visible));

        public Brush Icon
        {
            get => (Brush)this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        public ImageSource IconImageSource
        {
            get => (ImageSource)this.GetValue(IconImageSourceProperty);
            set => this.SetValue(IconImageSourceProperty, value);
        }

        public ushort ServersCount
        {
            get => (ushort)this.GetValue(ServersCountProperty);
            set => this.SetValue(ServersCountProperty, value);
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public Visibility VisibilityDetails
        {
            get => (Visibility)this.GetValue(VisibilityDetailsProperty);
            set => this.SetValue(VisibilityDetailsProperty, value);
        }

        private static void IconBrushImageSourceChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ServerTabItemHeaderControl)d).Icon = new ImageBrush()
            {
                ImageSource = (ImageSource)e.NewValue
            };
        }
    }
}