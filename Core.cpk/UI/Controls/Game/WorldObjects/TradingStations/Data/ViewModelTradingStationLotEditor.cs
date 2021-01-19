namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelTradingStationLotEditor : BaseViewModel
    {
        private readonly List<IProtoItem> allItemsList;

        private readonly Action callbackCancel;

        private readonly Action callbackSave;

        private readonly List<IProtoItem> existingItemsList;

        private readonly TradingStationLot lot;

        private string searchText = string.Empty;

        private IProtoItem selectedProtoItem;

        public ViewModelTradingStationLotEditor(TradingStationLot lot, Action callbackSave, Action callbackCancel)
        {
            this.lot = lot;
            this.callbackCancel = callbackCancel;
            this.callbackSave = callbackSave;

            this.selectedProtoItem = lot.ProtoItem;
            this.PriceCoinShiny = lot.PriceCoinShiny;
            this.PriceCoinPenny = lot.PriceCoinPenny;
            this.LotQuantity = Math.Max((ushort)1, lot.LotQuantity);

            // no need to order as the ordering is applied later
            this.allItemsList = Api.FindProtoEntities<IProtoItem>().ToList();
            this.existingItemsList = this.GetExistingItemsList();

            this.RefreshLists();
        }

        public List<ViewItemWithIcon> AllItemsList { get; private set; }

        public BaseCommand CommandCancel => new ActionCommand(this.ExecuteCommandCancel);

        public BaseCommand CommandReset => new ActionCommand(this.ExecuteCommandReset);

        public BaseCommand CommandSave => new ActionCommand(this.ExecuteCommandSave);

        public List<ViewItemWithIcon> ExistingItemsList { get; private set; }

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.SelectedProtoItem?.Icon);

        public Brush IconCoinPenny => UITradingIcons.LazyIconCoinPenny.Value;

        public Brush IconCoinShiny => UITradingIcons.LazyIconCoinShiny.Value;

        public ushort LotQuantity { get; set; }

        public uint PriceCoinPenny { get; set; }

        public uint PriceCoinShiny { get; set; }

        public string SearchText
        {
            get => this.searchText;
            set
            {
                value = value?.TrimStart() ?? string.Empty;
                if (this.searchText == value)
                {
                    return;
                }

                this.searchText = value;
                this.NotifyThisPropertyChanged();

                this.RefreshLists();
            }
        }

        public IProtoItem SelectedProtoItem => this.selectedProtoItem;

        public ViewItemWithIcon SelectedProtoItemViewModel
        {
            get => new(this.selectedProtoItem);
            set
            {
                if (this.selectedProtoItem == value.ProtoItem)
                {
                    return;
                }

                this.selectedProtoItem = value.ProtoItem;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.SelectedProtoItem));
                this.NotifyPropertyChanged(nameof(this.Icon));
            }
        }

        private List<ViewItemWithIcon> ApplyFilter(List<IProtoItem> items)
        {
            // remove items we don't want to be tradable and displayed in the list

            // individual items:
            items.Remove(Api.GetProtoEntity<ItemImplantBroken>());

            // and all items without icons (which are not really actual items):
            items.RemoveAll(i => i.Icon is null);

            var search = this.searchText.Trim();
            if (search.Length > 0)
            {
                items = ProtoSearchHelper.SearchProto(items, search).ToList();
            }

            return ClientContainerSortHelper.SortItemPrototypes(items)
                                            .Select(i => new ViewItemWithIcon(i))
                                            .ToList();
        }

        private void ExecuteCommandCancel()
        {
            this.callbackCancel();
        }

        private void ExecuteCommandReset()
        {
            this.SelectedProtoItemViewModel = default;
            this.LotQuantity = 0;
            this.PriceCoinShiny = 0;
            this.PriceCoinPenny = 0;
        }

        private void ExecuteCommandSave()
        {
            this.callbackSave();
        }

        private List<IProtoItem> GetExistingItemsList()
        {
            var stockItemsContainer = this.lot.GameObject.GetPrivateState<ObjectTradingStationPrivateState>()
                                          .StockItemsContainer;

            var availableLots = this.lot.GameObject.GetPublicState<ObjectTradingStationPublicState>().Lots;

            var result = new List<IProtoItem>();
            result.AddRange(stockItemsContainer.Items.Select(i => i.ProtoItem));
            result.AddRange(availableLots.Select(l => l.ProtoItem).Where(i => i is not null));
            return result.Distinct().ToList();
        }

        private void RefreshLists()
        {
            this.ExistingItemsList = this.ApplyFilter(this.existingItemsList);
            this.AllItemsList = this.ApplyFilter(this.allItemsList);
        }
    }
}