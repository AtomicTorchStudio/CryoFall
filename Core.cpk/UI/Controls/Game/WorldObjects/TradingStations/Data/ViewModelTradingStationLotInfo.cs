namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.Systems.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelTradingStationLotInfo : BaseViewModel
    {
        private readonly TradingStationsMapMarksSystem.TradingStationLotInfo lot;

        public ViewModelTradingStationLotInfo(
            TradingStationsMapMarksSystem.TradingStationLotInfo lot)
        {
            this.lot = lot;
        }

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.lot.ProtoItem?.Icon);

        public Brush IconCoinPenny => UITradingIcons.LazyIconCoinPenny.Value;

        public Brush IconCoinShiny => UITradingIcons.LazyIconCoinShiny.Value;

        public bool IsAvailable => this.lot.State == TradingStationLotState.Available;

        public bool IsEnabled => this.lot.State != TradingStationLotState.Disabled;

        public ushort LotQuantity => this.lot.LotQuantity;

        public uint PriceCoinPenny => this.lot.PriceCoinPenny;

        public uint PriceCoinShiny => this.lot.PriceCoinShiny;

        public string ProblemText => this.GetProblemText();

        public IProtoItem ProtoItem => this.lot.ProtoItem;

        private string GetProblemText()
        {
            return this.lot.State.GetDescription();
        }
    }
}