namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class TaskCraftRecipe : BasePlayerTaskWithListAndCount<Recipe>
    {
        // Used in case the recipe could be crafted on a particular station (station name provided as {0}).
        // For example: Craft Torch on Workbench.
        public const string CraftOnStationNameTitleFormat = "on {0}";

        public const string DescriptionTitlePrefix = "Craft";

        public TaskCraftRecipe(
            IReadOnlyList<Recipe> list,
            ushort count,
            string description)
            : base(list,
                   count,
                   description)
        {
        }

        public override bool IsReversible => false;

        protected override string AutoDescription
        {
            get
            {
                var prefix = DescriptionTitlePrefix + ":";
                if (this.List.Count > 1)
                {
                    return prefix + " " + this.ListNames;
                }

                var recipe = this.List[0];
                var result = prefix + " " + recipe.Name;
                result = AppendRecipeLocationIfNecessary(result, recipe);
                return result;
            }
        }

        public static string AppendRecipeLocationIfNecessary(string result, Recipe recipe)
        {
            if (recipe is Recipe.BaseRecipeForStation recipeForStation
                && recipeForStation.RecipeType != RecipeType.Hand)
            {
                // list stations where the recipe might be crafted
                result += " ("
                          + string.Format(CraftOnStationNameTitleFormat,
                                          recipeForStation.StationTypes
                                                          .Select(s => s.Name)
                                                          .GetJoinedString())
                          + ")";
            }

            return result;
        }

        public static TaskCraftRecipe RequireHandRecipe<TRecipe>(
            ushort count = 1,
            string description = null)
            where TRecipe : Recipe.RecipeForHandCrafting
        {
            var list = Api.FindProtoEntities<TRecipe>();
            return new TaskCraftRecipe(list, count, description);
        }

        public static TaskCraftRecipe RequireStationRecipe<TRecipe>(
            ushort count = 1,
            string description = null)
            where TRecipe : Recipe.RecipeForStationCrafting, new()
        {
            var list = Api.FindProtoEntities<TRecipe>();
            return new TaskCraftRecipe(list, count, description);
        }

        public static TaskCraftRecipe RequireStationRecipe(
            List<Recipe.RecipeForStationCrafting> list,
            ushort count = 1,
            string description = null)
        {
            return new(list, count, description);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.List.Count == 1
                       ? this.List[0].Icon
                       : null;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                CraftingMechanics.ServerNonManufacturingRecipeCrafted += this.RecipeCraftedHandler;
            }
            else
            {
                CraftingMechanics.ServerNonManufacturingRecipeCrafted -= this.RecipeCraftedHandler;
            }
        }

        private void RecipeCraftedHandler(CraftingQueueItem craftingQueueItem)
        {
            if (craftingQueueItem.GameObject is not ICharacter character)
            {
                return;
            }

            var context = this.GetActiveContext(character, out var state);
            if (context is null)
            {
                return;
            }

            if (!this.List.Contains(craftingQueueItem.RecipeEntry.Recipe))
            {
                return;
            }

            state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
            context.Refresh();
        }
    }
}