namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowHandCrafting : BaseViewModel
    {
        private bool isActive;

        private Recipe lastSelectedRecipe;

        public ViewModelWindowHandCrafting()
        {
            if (IsDesignTime)
            {
                this.ViewModelCraftingMenu = new ViewModelCraftingMenu(
                    validateItemsAvailabilityInPlayerInventory: false);
                return;
            }

            var currentCharacter = Client.Characters.CurrentPlayerCharacter;
            this.ContainerInventory = (IClientItemsContainer)currentCharacter.SharedGetPlayerContainerInventory();
        }

        public IClientItemsContainer ContainerInventory { get; }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;

                if (this.isActive)
                {
                    var currentCharacter = Client.Characters.CurrentPlayerCharacter;
                    ClientContainersExchangeManager.Register(this,
                                                             currentCharacter.SharedGetPlayerContainerInventory());
                    ClientContainersExchangeManager.Register(this, currentCharacter.SharedGetPlayerContainerHotbar());
                    var recipes = GetAllRecipes();
                    RemoveLockedRecipes(recipes);

                    var recipesTotalCount = recipes.Count;
                    this.ViewModelCraftingMenu = new ViewModelCraftingMenu(
                        recipes,
                        recipesTotalCount,
                        validateItemsAvailabilityInPlayerInventory: true);
                    this.ViewModelCraftingMenu.TrySelectRecipe(this.lastSelectedRecipe);
                }
                else
                {
                    ClientContainersExchangeManager.Unregister(this);
                    var viewModelCraftingMenu = this.ViewModelCraftingMenu;
                    this.lastSelectedRecipe = viewModelCraftingMenu.SelectedRecipe?.Recipe;
                    this.ViewModelCraftingMenu = null;
                    viewModelCraftingMenu.Dispose();
                }

                this.NotifyThisPropertyChanged();
            }
        }

        public ViewModelCraftingMenu ViewModelCraftingMenu { get; private set; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.IsActive = false;
        }

        private static List<Recipe> GetAllRecipes()
        {
            return Recipe.AllRecipes
                         .Where(r => r.RecipeType == RecipeType.Hand)
                         .ToList();
        }

        private static void RemoveLockedRecipes(List<Recipe> list)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            list.RemoveAll(r => !r.SharedIsTechUnlocked(character));
        }
    }
}