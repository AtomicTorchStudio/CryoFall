namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.Items.Reactor;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelTradingStationLotEditor : BaseViewModel
    {
        private readonly List<IProtoItem> allItemsList;

        private readonly Action callbackCancel;

        private readonly Action callbackSave;

        private readonly List<IProtoItem> existingItemsList;

        private readonly TradingStationLot lot;

        private byte minQualityPercent;

        private string searchText = string.Empty;

        private IProtoItem selectedProtoItem;

        public ViewModelTradingStationLotEditor(
            TradingStationLot lot,
            Action callbackSave,
            Action callbackCancel)
        {
            this.lot = lot;
            this.IsStationBuying = ProtoObjectTradingStation.GetPublicState(
                                       (IStaticWorldObject)lot.GameObject).Mode
                                   == TradingStationMode.StationBuying;
            this.callbackCancel = callbackCancel;
            this.callbackSave = callbackSave;

            this.selectedProtoItem = lot.ProtoItem;
            this.PriceCoinShiny = lot.PriceCoinShiny;
            this.PriceCoinPenny = lot.PriceCoinPenny;
            this.LotQuantity = Math.Max((ushort)1, lot.LotQuantity);
            this.MinQualityPercent = lot.MinQualityPercent;

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

        public bool IsProtoItemStackable => this.selectedProtoItem?.IsStackable ?? false;

        public bool IsProtoItemWithDurability
            => this.selectedProtoItem is IProtoItemWithDurability { DurabilityMax: > 0 };

        public object IsProtoItemWithFreshness
            => this.selectedProtoItem is IProtoItemWithFreshness { FreshnessMaxValue: > 0 };

        public bool IsStationBuying { get; private set; }

        public ushort LotQuantity { get; set; }

        public byte MinQualityPercent
        {
            get => this.minQualityPercent;
            set
            {
                value = (byte)Math.Min((int)value, 100);
                if (this.minQualityPercent == value)
                {
                    return;
                }

                this.minQualityPercent = value;
                this.NotifyThisPropertyChanged();
            }
        }

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
                this.NotifyPropertyChanged(nameof(this.IsProtoItemStackable));
                this.NotifyPropertyChanged(nameof(this.IsProtoItemWithDurability));
                this.NotifyPropertyChanged(nameof(this.IsProtoItemWithFreshness));

                if (!this.IsProtoItemStackable)
                {
                    this.LotQuantity = 1;
                }
            }
        }

        private List<ViewItemWithIcon> ApplyFilter(List<IProtoItem> items)
        {
            // remove items we don't want to be tradable and displayed in the list

            // individual items:
            items.Remove(Api.GetProtoEntity<ItemImplantBroken>());
            items.Remove(Api.GetProtoEntity<ItemReactorBrokenModule>());

            // and all items without icons (which are not really actual items):
            items.RemoveAll(i => i.Icon is null);

            // replace skinned items with their base type
            for (var index = 0; index < items.Count; index++)
            {
                var protoItem = items[index];
                if (((IProtoItemWithSkinData)protoItem).BaseProtoItem is { } baseProtoItem)
                {
                    items[index] = baseProtoItem;
                }
            }

            var search = this.searchText.Trim();
            if (search.Length > 0)
            {
                items = ProtoSearchHelper.SearchProto(items, search).ToList();
            }

            items = items.Distinct().ToList();
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