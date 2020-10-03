namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelRecipesBrowserRecipeDetails : BaseViewModel, IViewModelWithRecipe
    {
        private static readonly IReadOnlyList<ProtoItemWithCount> EmptyItemsList =
            new List<ProtoItemWithCount>(0).AsReadOnly();

        private ushort countToCraft = 1;

        private bool isCanCraft;

        private ViewModelCraftingRecipe viewModelRecipe;

        public ViewModelRecipesBrowserRecipeDetails(Action onSelected, Action onCancel)
        {
            if (onSelected is not null)
            {
                this.VisibilityCommandSelect = Visibility.Visible;
                this.CommandSelect = new ActionCommandWithCondition(
                    onSelected,
                    checkCanExecute: () => this.IsCanCraft);
            }
            else
            {
                this.VisibilityCommandSelect = Visibility.Collapsed;
            }

            if (onCancel is not null)
            {
                this.VisibilityCommandCancel = Visibility.Visible;
                this.CommandCancel = new ActionCommand(onCancel);
            }
            else
            {
                this.VisibilityCommandCancel = Visibility.Collapsed;
            }
        }

        public ActionCommand CommandCancel { get; }

        public BaseCommand CommandSelect { get; }

        public ushort CountToCraft
        {
            get => this.countToCraft;
            set
            {
                if (this.countToCraft == value)
                {
                    return;
                }

                this.countToCraft = value;
                this.NotifyThisPropertyChanged();
                this.RefreshIsCanCraft();
            }
        }

        public IReadOnlyList<ProtoItemWithCount> InputItems { get; set; } = EmptyItemsList;

        public bool IsCanCraft
        {
            get => this.isCanCraft;
            set
            {
                if (this.isCanCraft == value)
                {
                    return;
                }

                this.isCanCraft = value;
                this.NotifyThisPropertyChanged();
                this.CommandSelect?.OnCanExecuteChanged();
            }
        }

        public IReadOnlyList<ProtoItemWithCount> OutputItems { get; set; } = EmptyItemsList;

        public ViewModelCraftingRecipe ViewModelRecipe
        {
            get => this.viewModelRecipe;
            set
            {
                if (this.viewModelRecipe == value)
                {
                    // refresh input/output items
                    this.NotifyPropertyChanged(nameof(this.InputItems));
                    this.NotifyPropertyChanged(nameof(this.OutputItems));
                    return;
                }

                this.viewModelRecipe = value;

                var recipe = value?.Recipe;
                this.InputItems = recipe?.InputItems ?? EmptyItemsList;
                this.OutputItems = recipe?.OutputItems.Items ?? EmptyItemsList;

                this.RefreshIsCanCraft();

                this.NotifyThisPropertyChanged();
            }
        }

        public Visibility VisibilityCommandCancel { get; }

        public Visibility VisibilityCommandSelect { get; }

        private void RefreshIsCanCraft()
        {
            var recipe = this.viewModelRecipe?.Recipe;
            if (recipe is null)
            {
                this.IsCanCraft = false;
                return;
            }

            this.IsCanCraft = true;
        }
    }
}