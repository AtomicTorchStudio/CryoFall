namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.TradingStations;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowTradingStationAdmin : BaseViewModel
    {
        private readonly IStaticWorldObject worldObjectTradingStation;

        private bool isStationSellingMode;

        public ViewModelWindowTradingStationAdmin(
            IStaticWorldObject worldObjectTradingStation,
            ObjectTradingStationPrivateState privateState,
            ObjectTradingStationPublicState publicState)
        {
            this.worldObjectTradingStation = worldObjectTradingStation;

            this.IsInsideFactionClaim = LandClaimSystem.SharedIsWorldObjectOwnedByFaction(worldObjectTradingStation);
            if (!this.IsInsideFactionClaim)
            {
                this.ViewModelOwnersEditor = new ViewModelWorldObjectOwnersEditor(
                    privateState.Owners,
                    callbackServerSetOwnersList:
                    ownersList => WorldObjectOwnersSystem.ClientSetOwners(worldObjectTradingStation, ownersList),
                    title: CoreStrings.ObjectOwnersList_Title);
            }

            this.ViewModelStockContainerExchange = new ViewModelItemsContainerExchange(
                privateState.StockItemsContainer);

            this.Lots = publicState.Lots.Select(l => new ViewModelTradingStationLot(l, this.LotEditorSaveHandler))
                                   .ToList();

            this.isStationSellingMode = publicState.Mode == TradingStationMode.StationSelling;
        }

        public bool IsInsideFactionClaim { get; }

        public bool IsStationSellingMode
        {
            get => this.isStationSellingMode;
            set
            {
                if (this.isStationSellingMode == value)
                {
                    return;
                }

                this.isStationSellingMode = value;
                this.NotifyThisPropertyChanged();

                TradingStationsSystem.ClientSendStationSetMode(
                    this.worldObjectTradingStation,
                    this.isStationSellingMode
                        ? TradingStationMode.StationSelling
                        : TradingStationMode.StationBuying);
            }
        }

        public IReadOnlyList<ViewModelTradingStationLot> Lots { get; }

        public ViewModelWorldObjectOwnersEditor ViewModelOwnersEditor { get; }

        public ViewModelItemsContainerExchange ViewModelStockContainerExchange { get; }

        private void LotEditorSaveHandler(TradingStationLot lot, ViewModelTradingStationLotEditor viewModelEditor)
        {
            TradingStationsSystem.ClientSendTradingLotModification(
                lot,
                viewModelEditor.SelectedProtoItem,
                viewModelEditor.LotQuantity,
                (ushort)Math.Min(TradingStationLot.MaxPrice, viewModelEditor.PriceCoinPenny),
                (ushort)Math.Min(TradingStationLot.MaxPrice, viewModelEditor.PriceCoinShiny));
        }
    }
}