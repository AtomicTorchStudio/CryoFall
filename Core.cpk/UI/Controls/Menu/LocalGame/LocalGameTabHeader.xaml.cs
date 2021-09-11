namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.LocalGame
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuLocalGameTabHeader : BaseUserControl
    {
        public static readonly DependencyProperty IconImageSourceProperty
            = DependencyProperty.Register(nameof(IconImageSource),
                                          typeof(ImageSource),
                                          typeof(MenuLocalGameTabHeader),
                                          new PropertyMetadata(default(ImageSource),
                                                               IconBrushImageSourceChangedHandler));

        public static readonly DependencyProperty IconProperty
            = DependencyProperty.Register(nameof(Icon),
                                          typeof(Brush),
                                          typeof(MenuLocalGameTabHeader),
                                          new PropertyMetadata(Brushes.DarkOrange));

        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register("Text",
                                          typeof(string),
                                          typeof(MenuLocalGameTabHeader),
                                          new PropertyMetadata(default(string)));

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

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        private static void IconBrushImageSourceChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuLocalGameTabHeader)d).Icon = new ImageBrush()
            {
                ImageSource = (ImageSource)e.NewValue
            };
        }
    }
}