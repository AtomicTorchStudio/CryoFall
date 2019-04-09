namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data;

    public partial class WindowTradingStationLotEditor : BaseUserControlWithWindow
    {
        private readonly TradingStationLot lot;

        private ViewModelTradingStationLotEditor viewModel;

        public WindowTradingStationLotEditor(TradingStationLot lot)
        {
            this.lot = lot;
        }

        public WindowTradingStationLotEditor()
        {
        }

        public ViewModelTradingStationLotEditor ViewModel => this.viewModel;

        protected override void OnLoaded()
        {
            base.OnLoaded();
            this.DataContext = this.viewModel =
                                   new ViewModelTradingStationLotEditor(
                                       this.lot,
                                       callbackSave: () => this.CloseWindow(DialogResult.OK),
                                       callbackCancel: () => this.CloseWindow(DialogResult.Cancel));
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}