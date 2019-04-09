namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class RequirementCraftRecipe : QuestRequirementWithList<Recipe>
    {
        // Used in case the recipe could be crafted on a particular station (station name provided as {0}).
        // For example: Craft Torch on Workbench.
        public const string CraftOnStationNameTitleFormat = "on {0}";

        public const string DescriptionTitlePrefix = "Craft";

        public RequirementCraftRecipe(
            IReadOnlyList<Recipe> list,
            ushort count,
            string description)
            : base(
                list,
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
        }

        public static RequirementCraftRecipe RequireHandRecipe<TRecipe>(
            ushort count = 1,
            string description = null)
            where TRecipe : Recipe.RecipeForHandCrafting
        {
            var list = Api.FindProtoEntities<TRecipe>();
            return new RequirementCraftRecipe(list, count, description);
        }

        public static RequirementCraftRecipe RequireStationRecipe<TRecipe>(
            ushort count = 1,
            string description = null)
            where TRecipe : Recipe.RecipeForStationCrafting, new()
        {
            var list = Api.FindProtoEntities<TRecipe>();
            return new RequirementCraftRecipe(list, count, description);
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
            if (!(craftingQueueItem.GameObject is ICharacter character))
            {
                return;
            }

            var context = this.GetActiveContext(character, out var state);
            if (context == null)
            {
                return;
            }

            if (!this.List.Contains(craftingQueueItem.Recipe))
            {
                return;
            }

            state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
            context.Refresh();
        }
    }
}