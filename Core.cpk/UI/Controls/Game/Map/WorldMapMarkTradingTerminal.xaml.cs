namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldMapMarkTradingTerminal : BaseUserControl
    {
        public static readonly DependencyProperty IsOwnerProperty =
            DependencyProperty.Register(nameof(IsOwner),
                                        typeof(bool),
                                        typeof(WorldMapMarkTradingTerminal),
                                        new PropertyMetadata(default(bool)));

        public WorldMapMarkTradingTerminal(bool isOwner)
        {
            this.IsOwner = isOwner;
        }

        public WorldMapMarkTradingTerminal()
        {
        }

        public bool IsOwner
        {
            get => (bool)this.GetValue(IsOwnerProperty);
            set => this.SetValue(IsOwnerProperty, value);
        }
    }
}