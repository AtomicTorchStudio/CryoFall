namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowCraftingStation : BaseUserControlWithWindow
    {
        private IProtoObjectCraftStation protoObjectCraftStation;

        public static WindowCraftingStation Instance { get; private set; }

        public WindowCraftingStationViewModel ViewModel { get; private set; }

        public static WindowCraftingStation Open(IProtoObjectCraftStation protoObjectCraftStation)
        {
            if (Instance != null
                && Instance.protoObjectCraftStation == protoObjectCraftStation)
            {
                return Instance;
            }

            var instance = new WindowCraftingStation();
            Instance = instance;
            instance.protoObjectCraftStation = protoObjectCraftStation;
            Api.Client.UI.LayoutRootChildren.Add(instance);
            return Instance;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            var recipes = this.GetAllRecipes();
            var recipesCountTotal = recipes.Count;
            this.RemoveLockedRecipes(recipes);

            this.DataContext = this.ViewModel = new WindowCraftingStationViewModel(
                                   recipes,
                                   recipesCountTotal);
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            this.DataContext = null;
            this.ViewModel.Dispose();
            this.ViewModel = null;
            Instance = null;
        }

        private List<Recipe> GetAllRecipes()
        {
            return Recipe.AllRecipes
                         .Where(r => r is Recipe.RecipeForStationCrafting stationRecipe1
                                     && stationRecipe1.StationTypes.Contains(this.protoObjectCraftStation))
                         .ToList();
        }

        private void RemoveLockedRecipes(List<Recipe> list)
        {
            var character = Api.Client.Characters.CurrentPlayerCharacter;
            list.RemoveAll(r => !r.SharedIsTechUnlocked(character));
        }
    }
}