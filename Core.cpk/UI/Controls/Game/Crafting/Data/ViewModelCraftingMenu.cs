namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelCraftingMenu : BaseViewModel
    {
        private ViewModelCraftingRecipe selectedRecipe;

        private ViewModelCraftingMenuRecipeDetails selectedRecipeDetails;

        public ViewModelCraftingMenu(
            IReadOnlyCollection<Recipe> recipes,
            int recipesCountTotal,
            bool validateItemsAvailabilityInPlayerInventory,
            Action<Recipe> customCallbackOnRecipeSelect = null)
        {
            this.RecipesCountTotal = recipesCountTotal;
            this.ValidateItemsAvailabilityInPlayerInventory = validateItemsAvailabilityInPlayerInventory;
            this.RecipesList = recipes.Select(
                    recipe => new ViewModelCraftingMenuRecipeDetails(
                            validateItemsAvailabilityInPlayerInventory,
                            customCallbackOnRecipeSelect)
                        {
                            ViewModelRecipe = new ViewModelCraftingRecipe(recipe)
                        }
                ).ToList();

            this.SelectedRecipeDetails = this.RecipesList.FirstOrDefault();
        }

        // for design-time only
        public ViewModelCraftingMenu(bool validateItemsAvailabilityInPlayerInventory)
            : this(CreateTestRecipesList(),
                   CreateTestRecipesList().Count,
                   validateItemsAvailabilityInPlayerInventory,
                   customCallbackOnRecipeSelect: null)
        {
            if (!IsDesignTime)
            {
                throw new Exception("Design-time only");
            }
        }

        public int RecipesCountTotal { get; }

        public int RecipesCountUnlocked => this.RecipesList.Count;

        public IReadOnlyList<ViewModelCraftingMenuRecipeDetails> RecipesList { get; }

        public Visibility RecipesListEmptyMessageVisibility
            => this.RecipesList.Count == 0
                   ? Visibility.Visible
                   : Visibility.Collapsed;

        public ViewModelCraftingRecipe SelectedRecipe
        {
            get => this.selectedRecipe;
            set
            {
                if (this.selectedRecipe == value)
                {
                    return;
                }

                this.selectedRecipe = value;
                this.NotifyThisPropertyChanged();

                if (value == null)
                {
                    this.SelectedRecipeDetails = null;
                    return;
                }

                this.SelectedRecipeDetails =
                    this.RecipesList.FirstOrDefault(r => r.ViewModelRecipe == this.selectedRecipe);
            }
        }

        public ViewModelCraftingMenuRecipeDetails SelectedRecipeDetails
        {
            get => this.selectedRecipeDetails;
            set
            {
                if (this.selectedRecipeDetails == value)
                {
                    return;
                }

                this.selectedRecipeDetails = value;
                this.NotifyThisPropertyChanged();

                this.SelectedRecipe = value?.ViewModelRecipe;
            }
        }

        public bool ValidateItemsAvailabilityInPlayerInventory { get; }

        public void TrySelectRecipe(Recipe recipe)
        {
            if (recipe == null)
            {
                return;
            }

            foreach (var vm in this.RecipesList)
            {
                if (recipe == vm.ViewModelRecipe.Recipe)
                {
                    // found the view model for this recipe
                    this.SelectedRecipe = vm.ViewModelRecipe;
                    return;
                }
            }
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            foreach (var viewModelCraftingMenuRecipeDetails in this.RecipesList)
            {
                viewModelCraftingMenuRecipeDetails.ViewModelRecipe.Dispose();
                viewModelCraftingMenuRecipeDetails.Dispose();
            }
        }

        /// <summary>
        /// Some design-time data for XAML designer. This is not used in actual game.
        /// </summary>
        private static List<Recipe> CreateTestRecipesList()
        {
            return new List<Recipe>()
            {
                new RecipeToolboxT1(),
                new RecipeAxeIron(),
                new RecipeJamFromBerriesRed()
            };
        }
    }
}