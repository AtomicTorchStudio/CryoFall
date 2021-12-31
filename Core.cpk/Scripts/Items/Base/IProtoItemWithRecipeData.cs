namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemWithRecipeData :IProtoItem
    {
        IReadOnlyList<Recipe> ListedInRecipes { get; }

        void PrepareProtoItemLinkRecipe(Recipe recipe);
    }
}