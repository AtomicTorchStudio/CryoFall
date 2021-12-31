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
        private readonly List<ViewModelCraftingMenuRecipeDetails> allRecipes;

        private ViewModelCraftingRecipe selectedListEntry;

        private ViewModelCraftingMenuRecipeDetails selectedListEntryDetails;

        private Recipe selectedRecipe;

        public ViewModelCraftingMenu(
            IReadOnlyCollection<Recipe> recipes,
            int recipesCountTotal,
            bool validateItemsAvailabilityInPlayerInventory,
            Action<RecipeWithSkin> customCallbackOnRecipeSelect = null)
        {
            this.RecipesCountTotal = recipesCountTotal;
            this.ValidateItemsAvailabilityInPlayerInventory = validateItemsAvailabilityInPlayerInventory;

            this.allRecipes = recipes.Select(
                    recipe => new ViewModelCraftingMenuRecipeDetails(
                            validateItemsAvailabilityInPlayerInventory,
                            customCallbackOnRecipeSelect)
                        {
                            ViewModelRecipe = new ViewModelCraftingRecipe(recipe)
                        }
                ).ToList();

            var allRecipesWithoutSkins = this.allRecipes.ToList();
            allRecipesWithoutSkins.RemoveAll(r => r.ViewModelRecipe.Recipe.BaseRecipe is not null);
            this.RecipesList = allRecipesWithoutSkins;

            this.SelectedListEntry = this.RecipesList.FirstOrDefault()?.ViewModelRecipe;
        }

        // for design-time only
        public ViewModelCraftingMenu(bool validateItemsAvailabilityInPlayerInventory)
            : this(CreateTestRecipesList(),
                   CreateTestRecipesList().Count,
                   validateItemsAvailabilityInPlayerInventory)
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

        public ViewModelCraftingRecipe SelectedListEntry
        {
            get => this.selectedListEntry;
            set
            {
                if (this.selectedListEntry == value)
                {
                    return;
                }

                this.selectedListEntry = value;
                this.NotifyThisPropertyChanged();
                this.SelectedRecipe = value?.Recipe;

                this.SelectedListEntryDetails =
                    this.selectedListEntry is not null
                        ? this.allRecipes.FirstOrDefault(r => r.ViewModelRecipe == this.selectedListEntry)
                        : null;
            }
        }

        public ViewModelCraftingMenuRecipeDetails SelectedListEntryDetails
        {
            get => this.selectedListEntryDetails;
            set
            {
                if (this.selectedListEntryDetails == value)
                {
                    return;
                }

                this.selectedListEntryDetails = value;
                this.NotifyThisPropertyChanged();

                this.SelectedListEntry = value?.ViewModelRecipe;
            }
        }

        public Recipe SelectedRecipe
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

                this.SelectedRecipeDetails
                    = this.selectedRecipe is not null
                          ? this.allRecipes.FirstOrDefault(
                              r => r.ViewModelRecipe.Recipe == this.selectedRecipe)
                          : null;
            }
        }

        public ViewModelCraftingMenuRecipeDetails SelectedRecipeDetails { get; set; }

        public bool ValidateItemsAvailabilityInPlayerInventory { get; }

        public void TrySelectRecipe(Recipe recipe)
        {
            if (recipe is null)
            {
                return;
            }

            foreach (var vm in this.RecipesList)
            {
                if (recipe == vm.ViewModelRecipe.Recipe)
                {
                    // found the view model for this recipe
                    this.SelectedListEntry = vm.ViewModelRecipe;
                    return;
                }
            }
        }

        protected override void DisposeViewModel()
        {
            foreach (var viewModelCraftingMenuRecipeDetails in this.allRecipes)
            {
                viewModelCraftingMenuRecipeDetails.ViewModelRecipe.Dispose();
                viewModelCraftingMenuRecipeDetails.Dispose();
            }

            base.DisposeViewModel();
        }

        /// <summary>
        /// Some design-time data for XAML designer. This is not used in actual game.
        /// </summary>
        private static List<Recipe> CreateTestRecipesList()
        {
            return new()
            {
                new RecipeToolboxT1(),
                new RecipeAxeIron(),
                new RecipeJamFromBerriesRed()
            };
        }
    }
}