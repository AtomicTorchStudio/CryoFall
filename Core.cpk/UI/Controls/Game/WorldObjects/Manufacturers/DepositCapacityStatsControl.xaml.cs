namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class DepositCapacityStatsControl : BaseUserControl
    {
        public static readonly DependencyProperty BarBrushProperty =
            DependencyProperty.Register(nameof(BarBrush),
                                        typeof(Brush),
                                        typeof(DepositCapacityStatsControl),
                                        new PropertyMetadata(defaultValue: Brushes.Red));

        public Brush BarBrush
        {
            get => (Brush)this.GetValue(BarBrushProperty);
            set => this.SetValue(BarBrushProperty, value);
        }
    }
}