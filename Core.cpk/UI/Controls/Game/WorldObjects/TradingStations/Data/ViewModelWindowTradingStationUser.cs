namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowTradingStationUser : BaseViewModel
    {
        private readonly ObjectTradingStationPublicState publicState;

        private readonly IStaticWorldObject worldObjectTradingStation;

        private IReadOnlyList<ViewModelTradingStationLot> lots;

        public ViewModelWindowTradingStationUser(
            IStaticWorldObject worldObjectTradingStation,
            ObjectTradingStationPublicState publicState)
        {
            this.worldObjectTradingStation = worldObjectTradingStation;
            this.publicState = publicState;

            this.lots = publicState.Lots.Select(l => new ViewModelTradingStationLot(l))
                                   .ToList();

            publicState.ClientSubscribe(_ => _.Mode,
                                        _ => this.NotifyPropertyChanged(nameof(this.IsStationSellingMode)),
                                        this);
        }

        public bool IsStationSellingMode => this.publicState.Mode == TradingStationMode.StationSelling;

        public IReadOnlyList<ViewModelTradingStationLot> Lots => this.lots;

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            foreach (var lot in this.lots)
            {
                lot.Dispose();
            }

            this.lots = null;
        }
    }
}