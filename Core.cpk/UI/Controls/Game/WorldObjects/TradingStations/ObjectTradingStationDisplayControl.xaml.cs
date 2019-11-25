namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ObjectTradingStationDisplayControl : BaseUserControl
    {
        public static readonly DependencyProperty IsBuyModeProperty =
            DependencyProperty.Register("IsBuyMode",
                                        typeof(bool),
                                        typeof(ObjectTradingStationDisplayControl),
                                        new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty RowsNumberProperty =
            DependencyProperty.Register("RowsNumber",
                                        typeof(int),
                                        typeof(ObjectTradingStationDisplayControl),
                                        new PropertyMetadata(defaultValue: 2));

        private ItemsControl itemsControl;

        private IReadOnlyCollection<TradingStationsMapMarksSystem.TradingStationLotInfo> tradingLots;

        private List<ViewModelTradingStationLotInfo> viewModels;

        public bool IsBuyMode
        {
            get => (bool)this.GetValue(IsBuyModeProperty);
            set => this.SetValue(IsBuyModeProperty, value);
        }

        public int RowsNumber
        {
            get => (int)this.GetValue(RowsNumberProperty);
            set => this.SetValue(RowsNumberProperty, value);
        }

        public IReadOnlyCollection<TradingStationsMapMarksSystem.TradingStationLotInfo> TradingLots
        {
            get => this.tradingLots;
            set
            {
                if (this.tradingLots == value)
                {
                    return;
                }

                this.tradingLots = value;
                this.Refresh();
            }
        }

        protected override void InitControl()
        {
            this.itemsControl = this.GetByName<ItemsControl>("ItemsControl");
        }

        protected override void OnLoaded()
        {
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.DestroyViewModels();
        }

        private void DestroyViewModels()
        {
            if (this.viewModels is null)
            {
                return;
            }

            this.itemsControl.Items.Clear();

            foreach (var viewModel in this.viewModels)
            {
                viewModel.Dispose();
            }

            this.viewModels = null;
        }

        private void Refresh()
        {
            this.DestroyViewModels();

            if (!this.isLoaded
                || this.TradingLots is null)
            {
                return;
            }

            this.viewModels = new List<ViewModelTradingStationLotInfo>();
            var items = this.itemsControl.Items;

            foreach (var lot in this.TradingLots)
            {
                var viewModel = new ViewModelTradingStationLotInfo(lot);
                this.viewModels.Add(viewModel);
                items.Add(viewModel);
            }
        }
    }
};