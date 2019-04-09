namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ManufacturingConfig
    {
        public readonly bool IsAutoSelectRecipe;

        public readonly bool IsProduceByproducts;

        public ManufacturingConfig(
            IProtoStaticWorldObject protoManufacturer,
            IEnumerable<Recipe> recipes,
            IEnumerable<Recipe.RecipeForManufacturingByproduct> recipesForByproducts,
            bool isProduceByproducts,
            bool isAutoSelectRecipe)
        {
            var enabledRecipes = recipes.ToList();
            enabledRecipes.RemoveAll(r => !r.IsEnabled);
            this.Recipes = enabledRecipes;
            if (this.Recipes.Count == 0)
            {
                Api.Logger.Error(
                    "Recipes list for manufacturing is empty (perhaps recipes are disabled?): "
                    + protoManufacturer);
            }

            if (recipesForByproducts == null)
            {
                recipesForByproducts = new List<Recipe.RecipeForManufacturingByproduct>();
            }

            this.RecipesForByproducts = recipesForByproducts.ToList();
            this.IsProduceByproducts = this.RecipesForByproducts.Count > 0 && isProduceByproducts;
            this.IsAutoSelectRecipe = isAutoSelectRecipe;
        }

        public IReadOnlyCollection<Recipe> Recipes { get; }

        public IReadOnlyList<Recipe.RecipeForManufacturingByproduct> RecipesForByproducts { get; }

        public Recipe MatchRecipe(IWorldObject objectManufacturer, CraftingQueue craftingQueue)
        {
            Recipe bestRecipe = null;
            foreach (var recipe in this.Recipes)
            {
                if (bestRecipe != null
                    // TODO: compare by "value" of recipes?
                    && recipe.InputItems.Count < bestRecipe.InputItems.Count)
                {
                    // no need to check this recipe - already has a good recipe with the similar amount of items
                    continue;
                }

                if (recipe.CanBeCrafted(objectManufacturer, craftingQueue, countToCraft: 1))
                {
                    bestRecipe = recipe;
                }
            }

            return bestRecipe;
        }

        public Recipe.RecipeForManufacturingByproduct MatchRecipeForByproduct(IProtoItem protoItemFuel)
        {
            foreach (var recipe in this.RecipesForByproducts)
            {
                if (recipe.CanBeCrafted(protoItemFuel))
                {
                    return recipe;
                }
            }

            return null;
        }
    }
}