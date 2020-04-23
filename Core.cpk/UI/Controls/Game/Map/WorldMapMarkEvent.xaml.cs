namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldMapMarkEvent : BaseUserControl
    {
        public static readonly DependencyProperty EllipseColorEndProperty =
            DependencyProperty.Register("EllipseColorEnd",
                                        typeof(Color),
                                        typeof(WorldMapMarkEvent),
                                        new PropertyMetadata(default(Color)));

        public static readonly DependencyProperty EllipseColorStartProperty =
            DependencyProperty.Register("EllipseColorStart",
                                        typeof(Color),
                                        typeof(WorldMapMarkEvent),
                                        new PropertyMetadata(default(Color)));

        public static readonly DependencyProperty EllipseColorStrokeProperty =
            DependencyProperty.Register("EllipseColorStroke",
                                        typeof(Color),
                                        typeof(WorldMapMarkEvent),
                                        new PropertyMetadata(default(Color)));

        public Color EllipseColorEnd
        {
            get => (Color)this.GetValue(EllipseColorEndProperty);
            set => this.SetValue(EllipseColorEndProperty, value);
        }

        public Color EllipseColorStart
        {
            get => (Color)this.GetValue(EllipseColorStartProperty);
            set => this.SetValue(EllipseColorStartProperty, value);
        }

        public Color EllipseColorStroke
        {
            get => (Color)this.GetValue(EllipseColorStrokeProperty);
            set => this.SetValue(EllipseColorStrokeProperty, value);
        }
    }
}