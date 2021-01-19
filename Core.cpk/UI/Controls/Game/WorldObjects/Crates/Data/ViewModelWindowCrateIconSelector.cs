namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Crates.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowCrateIconSelector : BaseViewModel
    {
        private readonly List<IProtoItem> allItemsList;

        private readonly Action callbackCancel;

        private readonly Action callbackSave;

        private readonly IReadOnlyCollection<IItem> existingItems;

        private readonly List<IProtoItem> existingItemsList;

        private string searchText = string.Empty;

        private IProtoEntity selectedProtoEntity;

        public ViewModelWindowCrateIconSelector(
            IProtoEntity selectedProtoEntity,
            IReadOnlyCollection<IItem> existingItems,
            Action callbackSave,
            Action callbackCancel)
        {
            this.callbackCancel = callbackCancel;
            this.callbackSave = callbackSave;

            this.selectedProtoEntity = selectedProtoEntity;
            this.existingItems = existingItems;

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

        public Brush Icon
        {
            get
            {
                var icon = ClientCrateIconHelper.GetOriginalIcon(this.selectedProtoEntity);
                return icon is null
                           ? null
                           : Api.Client.UI.GetTextureBrush(icon);
            }
        }

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

        public IProtoEntity SelectedProtoEntity => this.selectedProtoEntity;

        public ViewItemWithIcon SelectedProtoItemViewModel
        {
            get => new(this.selectedProtoEntity as IProtoItem);
            set
            {
                if (this.selectedProtoEntity == value.ProtoItem)
                {
                    return;
                }

                this.selectedProtoEntity = value.ProtoItem;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.SelectedProtoEntity));
                this.NotifyPropertyChanged(nameof(this.Icon));
            }
        }

        private List<ViewItemWithIcon> ApplyFilter(List<IProtoItem> items)
        {
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
        }

        private void ExecuteCommandSave()
        {
            this.callbackSave();
        }

        private List<IProtoItem> GetExistingItemsList()
        {
            var result = new List<IProtoItem>();
            result.AddRange(this.existingItems.Select(i => i.ProtoItem));
            return result.Distinct().ToList();
        }

        private void RefreshLists()
        {
            this.ExistingItemsList = this.ApplyFilter(this.existingItemsList);
            this.AllItemsList = this.ApplyFilter(this.allItemsList);
        }
    }
}