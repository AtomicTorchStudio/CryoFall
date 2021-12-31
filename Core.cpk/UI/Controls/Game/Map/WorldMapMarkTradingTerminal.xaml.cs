namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldMapMarkTradingTerminal : BaseUserControl
    {
        public static readonly DependencyProperty IsOwnerProperty =
            DependencyProperty.Register(nameof(IsOwner),
                                        typeof(bool),
                                        typeof(WorldMapMarkTradingTerminal),
                                        new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty TooltipControlProperty =
            DependencyProperty.Register(nameof(TooltipControl),
                                        typeof(Control),
                                        typeof(WorldMapMarkTradingTerminal),
                                        new PropertyMetadata(default(Control)));

        private readonly uint tradingStationId;

        public WorldMapMarkTradingTerminal(uint tradingStationId, bool isOwner)
        {
            this.tradingStationId = tradingStationId;
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

        public Control TooltipControl
        {
            get => (Control)this.GetValue(TooltipControlProperty);
            set => this.SetValue(TooltipControlProperty, value);
        }

        protected override void InitControl()
        {
            ((FrameworkElement)ToolTipServiceExtend.GetToolTip(this)).DataContext = this;
            this.DataContext = this;
        }

        protected override void OnLoaded()
        {
            this.MouseEnter += this.MouseEnterHandler;
        }

        protected override void OnUnloaded()
        {
            this.MouseLeave += this.MouseLeaveHandler;
            this.DestroyTooltip();
        }

        private void DestroyTooltip()
        {
            this.TooltipControl = null;
        }

        private void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            this.TooltipControl = new WorldMapMarkTradingTerminalTooltip(this.tradingStationId);
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            this.DestroyTooltip();
        }
    }
}