namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowConstructionMenu : BaseViewModel
    {
        public Action<IProtoObjectStructure> StructureSelectedCallback;

        private ViewModelWindowConstructionMenuHotbarHelper hotbarHelper;

        private string searchText = string.Empty;

        private ViewModelStructureCategory selectedCategory;

        private ViewModelStructure selectedStructure;

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public ViewModelWindowConstructionMenu()
        {
            if (IsDesignTime)
            {
                this.StructuresList = new ViewModelStructuresSelectList();
                this.Categories = new List<ViewModelStructureCategory>()
                {
                    new ViewModelStructureCategory("Test category"),
                    new ViewModelStructureCategory("Test category 2"),
                    new ViewModelStructureCategory("Test category 3"),
                };
                this.selectedCategory = this.Categories[0];
                return;
            }

            this.CommandBuild = new ActionCommand(this.ExecuteCommandBuild);
            this.Categories = this.GetCategories();
            // select first category by default (if it's not available, the first available category will be selected automatically)
            this.SelectedCategory = this.Categories[0];

            this.hotbarHelper = new ViewModelWindowConstructionMenuHotbarHelper(
                onHotbarUpdated: hotbarContainer => this.UpdateToolRequirement());
            this.UpdateToolRequirement();
        }

        public IList<ViewModelStructureCategory> Categories { get; }

        public BaseCommand CommandBuild { get; }

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

                this.UpdateStructuresList();
            }
        }

        public ViewModelStructureCategory SelectedCategory
        {
            get => this.selectedCategory;
            set
            {
                if (this.selectedCategory == value)
                {
                    return;
                }

                if (this.selectedCategory != null)
                {
                    this.selectedCategory.IsSelected = false;
                }

                this.selectedCategory = value;

                if (this.selectedCategory != null)
                {
                    this.selectedCategory.IsSelected = true;
                }

                this.SearchText = string.Empty;

                this.NotifyThisPropertyChanged();
                this.UpdateStructuresList();
            }
        }

        public ViewModelStructure SelectedStructure
        {
            get => this.selectedStructure;
            set
            {
                if (this.selectedStructure == value)
                {
                    return;
                }

                if (this.selectedStructure != null)
                {
                    this.selectedStructure.IsSelected = false;
                }

                this.selectedStructure = value;

                if (this.selectedStructure != null)
                {
                    this.selectedStructure.IsSelected = true;
                }

                this.NotifyThisPropertyChanged();
                this.UpdateStructureDetailsView();
            }
        }

        public ViewModelStructuresSelectList StructuresList { get; private set; }

        public Visibility VisibilityBuildButtons { get; set; } = Visibility.Visible;

        public Visibility VisibilityDetailsView { get; private set; }

        public Visibility VisibilityPleaseEquipTool { get; set; } = Visibility.Visible;

        public void SelectStructure(IProtoObjectStructure protoStructure)
        {
            if (protoStructure == null)
            {
                return;
            }

            if (!this.GetAvailableStructures().Contains(protoStructure))
            {
                return;
            }

            this.SelectedCategory = this.Categories
                                        .First(vm => vm.Category == protoStructure.Category);
            this.SelectedStructure = this.StructuresList.Items
                                         .FirstOrDefault(vm => vm.ProtoStructure == protoStructure);
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.hotbarHelper.Dispose();
            this.hotbarHelper = null;
        }

        private void ExecuteCommandBuild()
        {
            if (this.VisibilityBuildButtons == Visibility.Visible)
            {
                this.StructureSelectedCallback(this.selectedStructure.ProtoStructure);
            }
        }

        private ICollection<IProtoObjectStructure> GetAvailableStructures()
        {
            var result = StructuresHelper.ClientGetAvailableToBuildStructures();
            // update categories
            var grouppedByCategory = result.GroupBy(r => r.Category)
                                           .ToDictionary(r => r.Key);
            foreach (var viewModelCategory in this.Categories)
            {
                var isAvailable = grouppedByCategory.ContainsKey(viewModelCategory.Category);
                viewModelCategory.IsEnabled = isAvailable;
            }

            return result;
        }

        private IList<ViewModelStructureCategory> GetCategories()
        {
            return Api.FindProtoEntities<ProtoStructureCategory>()
                      .OrderBy(c => c.Order)
                      .Select(c => new ViewModelStructureCategory(c))
                      .ToList();
        }

        private void UpdateStructureDetailsView()
        {
            if (this.selectedStructure == null)
            {
                this.VisibilityDetailsView = Visibility.Collapsed;
                return;
            }

            this.VisibilityDetailsView = Visibility.Visible;
        }

        private void UpdateStructuresList()
        {
            IEnumerable<IProtoObjectStructure> structuresEnumeration = this.GetAvailableStructures();
            if (this.selectedCategory == null
                || !this.selectedCategory.IsEnabled)
            {
                this.SelectedCategory = this.Categories.FirstOrDefault(c => c.IsEnabled);
                // category has been re-selected so this method was already executed again, no need to continue
                return;
            }

            var search = this.searchText.Trim();
            var hasSearchText = search.Length > 0;
            if (hasSearchText)
            {
                structuresEnumeration = ProtoSearchHelper.SearchProto(structuresEnumeration, search);
            }
            else
            {
                var category = this.selectedCategory?.Category;
                if (category != null)
                {
                    structuresEnumeration = structuresEnumeration.Where(s => s.Category == category);
                }
            }

            //// uncomment for scrollviewer test (long list of all structures in game)
            //structuresEnumeration
            //    = ClientConstructionStructuresListProvider.ClientGetAvailableToBuildStructures();

            structuresEnumeration = structuresEnumeration.OrderBy(s => s.Id);

            var oldStructuresList = this.StructuresList;
            this.StructuresList = null;
            oldStructuresList?.Items.Clear();
            oldStructuresList?.Dispose();
            this.StructuresList = new ViewModelStructuresSelectList(structuresEnumeration);

            this.SelectedStructure = this.StructuresList.Items.FirstOrDefault();
        }

        private void UpdateToolRequirement()
        {
            var hasTool = this.hotbarHelper.HotbarContainer.Items.Any(i => i.ProtoItem is IProtoItemToolToolbox);
            this.VisibilityPleaseEquipTool = !hasTool ? Visibility.Visible : Visibility.Collapsed;
            this.VisibilityBuildButtons = hasTool ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}