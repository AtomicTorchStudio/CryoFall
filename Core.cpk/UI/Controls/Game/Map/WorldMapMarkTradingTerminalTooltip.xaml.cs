namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldMapMarkTradingTerminalTooltip : BaseUserControl
    {
        private readonly uint tradingStationId;

        private LoadingDisplayControl loadingDisplayControl;

        private ObjectTradingStationDisplayControl tradingStationDisplayControl;

        public WorldMapMarkTradingTerminalTooltip(uint tradingStationId)
        {
            this.tradingStationId = tradingStationId;
        }

        protected override void InitControl()
        {
            this.loadingDisplayControl = this.GetByName<LoadingDisplayControl>("LoadingDisplayControl");
            this.tradingStationDisplayControl =
                this.GetByName<ObjectTradingStationDisplayControl>("TradingStationDisplayControl");
            this.tradingStationDisplayControl.Visibility = Visibility.Collapsed;
            this.loadingDisplayControl.Visibility = Visibility.Visible;
        }

        protected override async void OnLoaded()
        {
            var result = await TradingStationsMapMarksSystem.ClientRequestTradingStationInfo(this.tradingStationId);
            if (!this.isLoaded)
            {
                return;
            }

            this.tradingStationDisplayControl.RowsNumber = result.ActiveLots.Count > 3 ? 2 : 1;
            this.tradingStationDisplayControl.IsBuyMode = result.IsBuying;
            this.tradingStationDisplayControl.TradingLots = result.ActiveLots;
            this.tradingStationDisplayControl.Visibility = Visibility.Visible;
            this.loadingDisplayControl.Visibility = Visibility.Collapsed;
        }

        protected override void OnUnloaded()
        {
            this.tradingStationDisplayControl.Visibility = Visibility.Collapsed;
            this.loadingDisplayControl.Visibility = Visibility.Visible;
        }
    }
}