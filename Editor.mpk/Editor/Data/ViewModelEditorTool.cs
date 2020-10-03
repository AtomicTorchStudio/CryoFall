namespace AtomicTorch.CBND.CoreMod.Editor.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelEditorTool : BaseViewModel
    {
        public readonly BaseEditorTool Tool;

        public Action<ViewModelEditorTool> OnIsSelectedChanged;

        private readonly IReadOnlyList<ViewModelEditorToolItem> allItemsCollection;

        private bool isSelected;

        private string searchText;

        private ViewModelEditorToolItemFilter selectedFilter;

        private ViewModelEditorToolItem selectedItem;

        public ViewModelEditorTool(BaseEditorTool tool)
        {
            this.Tool = tool;
            if (IsDesignTime)
            {
                return;
            }

            var itemViewModels = tool.AbstractItems.Select(i => tool.CreateItemViewModel(i)).ToList();
            if (itemViewModels.Count > 0)
            {
                foreach (var viewModelEditorToolItem in itemViewModels)
                {
                    viewModelEditorToolItem.IsSelectedChanged += this.ItemIsSelectedChangedHandler;
                }

                this.ItemsVisibility = Visibility.Visible;
            }
            else
            {
                this.ItemsVisibility = Visibility.Collapsed;
            }

            var filterViewModels = tool.AbstractFilters.Select(f => new ViewModelEditorToolItemFilter(f)).ToList();
            foreach (var filter in filterViewModels)
            {
                filter.OnIsSelectedChanged = this.FilterOnIsSelectedChanged;
            }

            this.FiltersCollection = filterViewModels;
            this.allItemsCollection = itemViewModels;
            this.SelectDefaultFilter();
        }

        public SuperObservableCollection<ViewModelEditorToolItem> FilteredItemsCollection { get; }
            = new SuperObservableCollection<ViewModelEditorToolItem>();

        public IReadOnlyList<ViewModelEditorToolItemFilter> FiltersCollection { get; }

        public Visibility FiltersVisibility => this.FiltersCollection.Count > 1
                                                   ? Visibility.Visible
                                                   : Visibility.Collapsed;

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.Tool?.Icon);

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;
                this.NotifyThisPropertyChanged();
                this.OnIsSelectedChanged?.Invoke(this);

                if (this.isSelected)
                {
                    //this.SearchText = null;
                    //this.SelectedFilter = null;

                    // temporary disable
                    this.isSelected = false;
                    this.RebuildFilteredItemsCollection();
                    // enable again
                    this.isSelected = true;

                    this.CreateSettings();
                    EditorActiveToolManager.SetActiveTool(this.Tool, this.selectedItem?.ToolItem);
                }
                else
                {
                    //this.SearchText = null;
                    this.DestroySettings();
                }
            }
        }

        public Visibility ItemsVisibility { get; }

        public string Name => this.Tool.Name;

        public string SearchText
        {
            get => this.searchText;
            set
            {
                if (value == string.Empty)
                {
                    value = null;
                }

                if (this.searchText == value)
                {
                    return;
                }

                this.searchText = value;

                if (value is not null)
                {
                    this.SelectDefaultFilter();
                }

                this.NotifyThisPropertyChanged();

                if (this.isSelected)
                {
                    this.RebuildFilteredItemsCollection();
                }
            }
        }

        public ViewModelEditorToolItemFilter SelectedFilter
        {
            get => this.selectedFilter;
            set
            {
                value ??= this.FiltersCollection[0];
                if (this.selectedFilter == value)
                {
                    return;
                }

                this.selectedFilter = value;

                if (this.selectedFilter is not null)
                {
                    this.selectedFilter.IsSelected = true;
                }

                foreach (var filter in this.FiltersCollection)
                {
                    if (filter != this.selectedFilter)
                    {
                        filter.IsSelected = false;
                    }
                }

                if (this.selectedFilter != this.FiltersCollection[0])
                {
                    this.SearchText = null;
                }

                this.NotifyThisPropertyChanged();
                this.RebuildFilteredItemsCollection();
            }
        }

        public ViewModelEditorToolItem SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (this.selectedItem == value)
                {
                    return;
                }

                this.selectedItem = value;

                if (this.selectedItem is not null)
                {
                    this.selectedItem.IsSelected = true;
                }

                foreach (var item in this.allItemsCollection)
                {
                    if (item != this.selectedItem)
                    {
                        item.IsSelected = false;
                    }
                }

                if (this.isSelected)
                {
                    EditorActiveToolManager.SetActiveTool(this.Tool, this.selectedItem?.ToolItem);
                }

                this.NotifyThisPropertyChanged();
            }
        }

        public Visibility SettingsVisibility => this.Tool.HasSettings ? Visibility.Visible : Visibility.Collapsed;

        public FrameworkElement ToolSettingsControl { get; private set; }

        private void CreateSettings()
        {
            if (this.ToolSettingsControl is not null)
            {
                // do not destroy settings (cache it)
                //this.DestroySettings();
                return;
            }

            this.ToolSettingsControl = this.Tool.CreateSettingsControl();
        }

        private void DestroySettings()
        {
            // do not destroy settings (cache it)
            //this.ToolSettingsControl = null;
        }

        private void FilterOnIsSelectedChanged(ViewModelEditorToolItemFilter filter)
        {
            if (filter.IsSelected)
            {
                this.SelectedFilter = filter;
            }
        }

        private void ItemIsSelectedChangedHandler(ViewModelEditorToolItem toolItem)
        {
            if (toolItem.IsSelected)
            {
                this.SelectedItem = toolItem;
            }
        }

        private void RebuildFilteredItemsCollection()
        {
            this.FilteredItemsCollection.Clear();

            //if (!this.isSelected)
            //{
            //	return;
            //}

            if (this.searchText is not null)
            {
                // apply filter
                this.FilteredItemsCollection.AddRange(
                    this.allItemsCollection.Where(
                        i => i.ToolItem.Name.IndexOf(this.searchText, StringComparison.OrdinalIgnoreCase) >= 0
                             || i.ToolItem.Id.IndexOf(this.searchText, StringComparison.OrdinalIgnoreCase)
                             >= 0));
            }
            else
            {
                var filter = this.selectedFilter.Filter;
                this.FilteredItemsCollection.AddRange(
                    this.allItemsCollection.Where(item => filter.FilterItem(item.ToolItem)));
            }

            if (this.selectedItem is null
                || !this.FilteredItemsCollection.Contains(this.selectedItem))
            {
                // select first available item in collection
                this.SelectedItem = this.FilteredItemsCollection.FirstOrDefault();
            }
        }

        private void SelectDefaultFilter()
        {
            this.SelectedFilter = this.FiltersCollection[0];
        }
    }
}