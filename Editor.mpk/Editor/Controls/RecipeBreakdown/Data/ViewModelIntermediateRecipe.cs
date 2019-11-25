namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;

    public class ViewModelIntermediateRecipe : BaseViewModel
    {
        public ViewModelIntermediateRecipe(
            Recipe recipe,
            double multiplier)
        {
            this.ViewModelCraftingRecipe = new ViewModelCraftingRecipe(recipe);
            this.Multiplier = multiplier;
        }

        public double Multiplier { get; set; }

        public ViewModelCraftingRecipe ViewModelCraftingRecipe { get; }
    }
}