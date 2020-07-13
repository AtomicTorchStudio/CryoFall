namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data;

    public partial class WindowTradingStationLotEditor : BaseUserControlWithWindow
    {
        private static WindowTradingStationLotEditor Instance;

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

        public static void CloseWindowIfOpened()
        {
            Instance?.CloseWindow();
        }

        protected override void OnLoaded()
        {
            Instance = this;
            this.DataContext = this.viewModel =
                                   new ViewModelTradingStationLotEditor(
                                       this.lot,
                                       callbackSave: () => this.CloseWindow(DialogResult.OK),
                                       callbackCancel: () => this.CloseWindow(DialogResult.Cancel));
        }

        protected override void OnUnloaded()
        {
            if (ReferenceEquals(this, Instance))
            {
                Instance = null;
            }

            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}