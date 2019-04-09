namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class WindowCraftingStationViewModel : BaseViewModel
    {
        private static Recipe lastSelectedRecipe;

        public WindowCraftingStationViewModel(
            IReadOnlyCollection<Recipe> availableRecipes,
            int recipesCountTotal)
        {
            var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            this.ContainerInventory = (IClientItemsContainer)currentCharacter.SharedGetPlayerContainerInventory();
            this.ViewModelCraftingMenu = new ViewModelCraftingMenu(availableRecipes,
                                                                   recipesCountTotal,
                                                                   validateItemsAvailabilityInPlayerInventory: true);
            this.ViewModelCraftingMenu.TrySelectRecipe(lastSelectedRecipe);

            ClientContainersExchangeManager.Register(this, this.ContainerInventory);
            ClientContainersExchangeManager.Register(this, currentCharacter.SharedGetPlayerContainerHotbar());
        }

#if !GAME

        public WindowCraftingStationViewModel()
        {
        }

#endif

        public IClientItemsContainer ContainerInventory { get; }

        public ViewModelCraftingMenu ViewModelCraftingMenu { get; }

        protected override void DisposeViewModel()
        {
            lastSelectedRecipe = this.ViewModelCraftingMenu.SelectedRecipe?.Recipe;
        }
    }
}