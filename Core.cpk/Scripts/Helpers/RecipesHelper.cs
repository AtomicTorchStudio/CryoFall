namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class RecipesHelper
    {
        public static readonly IReadOnlyList<Recipe> AvailableRecipes;

        public static readonly HashSet<Type> BlacklistRecipes
            = new HashSet<Type>()
            {
                typeof(RecipeFibersFromPlastic),
                typeof(RecipeCoinPennyRecycle),
                typeof(RecipeCoinShinyRecycle)
            };

        static RecipesHelper()
        {
            var availableRecipes = new List<Recipe>();
            foreach (var recipe in Api.FindProtoEntities<Recipe>())
            {
                if (!recipe.IsEnabled
                    || recipe.InputItems.Count == 0
                    || recipe.OutputItems.Count == 0)
                {
                    continue;
                }

                if (recipe is IRecipeBarrelAddLiquid
                    || recipe is IRecipeBarrelRemoveLiquid)
                {
                    continue;
                }

                var recipeType = recipe.GetType();
                if (BlacklistRecipes.Contains(recipeType))
                {
                    continue;
                }
                
                switch (recipe.RecipeType)
                {
                    case RecipeType.Hand:
                    case RecipeType.StationCrafting:
                        availableRecipes.Add(recipe);
                        continue;

                    case RecipeType.Manufacturing:
                        availableRecipes.Add(recipe);
                        continue;

                    case RecipeType.ManufacturingByproduct:
                        // ignore
                        continue;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            AvailableRecipes = availableRecipes;
        }

        public static Recipe FindFirstRecipe(IProtoItem protoItem)
        {
            foreach (var availableRecipe in AvailableRecipes)
            {
                foreach (var outputItem in availableRecipe.OutputItems.Items)
                {
                    if (ReferenceEquals(outputItem.ProtoItem, protoItem))
                    {
                        // found a recipe
                        return availableRecipe;
                    }
                }
            }

            return null;
        }
    }
}