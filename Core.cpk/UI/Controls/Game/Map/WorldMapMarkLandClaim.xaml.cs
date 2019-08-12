namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldMapMarkLandClaim : BaseUserControl
    {
        public static readonly DependencyProperty IsFounderProperty =
            DependencyProperty.Register(nameof(IsFounder),
                                        typeof(bool),
                                        typeof(WorldMapMarkLandClaim),
                                        new PropertyMetadata(default(bool)));

        public bool IsFounder
        {
            get => (bool)this.GetValue(IsFounderProperty);
            set => this.SetValue(IsFounderProperty, value);
        }
    }
}