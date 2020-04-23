namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldMapMarkEventTooltip : BaseUserControl
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon),
                                        typeof(Brush),
                                        typeof(WorldMapMarkEventTooltip),
                                        new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text),
                                        typeof(string),
                                        typeof(WorldMapMarkEventTooltip),
                                        new PropertyMetadata(default(string)));

        public Brush Icon
        {
            get => (Brush)this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}