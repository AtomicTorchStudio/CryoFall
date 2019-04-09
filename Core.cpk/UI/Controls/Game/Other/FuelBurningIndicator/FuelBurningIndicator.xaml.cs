namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.FuelBurningIndicator
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FuelBurningIndicator : BaseUserControl
    {
        public static readonly DependencyProperty FuelColorProperty =
            DependencyProperty.Register(nameof(FuelColor),
                                        typeof(Color),
                                        typeof(FuelBurningIndicator),
                                        new PropertyMetadata(default(Color), ColorChangedHandler));

        public static readonly DependencyProperty FuelIconProperty =
            DependencyProperty.Register(nameof(FuelIcon),
                                        typeof(Brush),
                                        typeof(FuelBurningIndicator),
                                        new PropertyMetadata(default(Brush)));

        private RadialGradientBrush radialGradientBrush;

        public Color FuelColor
        {
            get => (Color)this.GetValue(FuelColorProperty);
            set => this.SetValue(FuelColorProperty, value);
        }

        public Brush FuelIcon
        {
            get => (Brush)this.GetValue(FuelIconProperty);
            set => this.SetValue(FuelIconProperty, value);
        }

        protected override void InitControl()
        {
            this.radialGradientBrush = (RadialGradientBrush)this.GetByName<Ellipse>("Glow").Fill;
        }

        protected override void OnLoaded()
        {
            this.Refresh();
        }

        private static void ColorChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FuelBurningIndicator)d).Refresh();
        }

        private void Refresh()
        {
            if (!this.isLoaded)
            {
                return;
            }

            var fuelColor = this.FuelColor;
            var gradientStopCollection = new GradientStopCollection();
            this.radialGradientBrush.GradientStops = gradientStopCollection;

            gradientStopCollection.Add(new GradientStop()
            {
                Color = Color.FromArgb(0x00, fuelColor.R, fuelColor.G, fuelColor.B),
                Offset = 1f
            });

            gradientStopCollection.Add(new GradientStop()
            {
                Color = Color.FromArgb(0x77, fuelColor.R, fuelColor.G, fuelColor.B),
                Offset = 0.25f
            });
        }
    }
}