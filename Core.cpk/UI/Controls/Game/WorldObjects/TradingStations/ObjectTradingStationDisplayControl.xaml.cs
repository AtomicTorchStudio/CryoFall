namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ObjectTradingStationDisplayControl : BaseUserControl
    {
        public static readonly DependencyProperty IsBuyModeProperty =
            DependencyProperty.Register("IsBuyMode",
                                        typeof(bool),
                                        typeof(ObjectTradingStationDisplayControl),
                                        new PropertyMetadata(default(bool)));

        private ItemsControl itemsControl;

        private List<ViewModelTradingStationLot> viewModels;

        public bool IsBuyMode
        {
            get => (bool)this.GetValue(IsBuyModeProperty);
            set => this.SetValue(IsBuyModeProperty, value);
        }

        public ObjectTradingStationPublicState StationPublicState { get; set; }

        protected override void InitControl()
        {
            this.itemsControl = this.GetByName<ItemsControl>("ItemsControl");
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            this.viewModels = new List<ViewModelTradingStationLot>();
            var items = this.itemsControl.Items;

            this.IsBuyMode = this.StationPublicState.Mode == TradingStationMode.StationBuying;
            foreach (var lot in this.StationPublicState.Lots)
            {
                var viewModel = new ViewModelTradingStationLot(lot);
                this.viewModels.Add(viewModel);
                items.Add(viewModel);
            }
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            this.itemsControl.Items.Clear();

            foreach (var viewModel in this.viewModels)
            {
                viewModel.Dispose();
            }

            this.viewModels = null;
        }
    }
};