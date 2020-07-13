namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementCraftAllTools : ProtoAchievement
    {
        public override string AchievementId => "craft_all_tools";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipeAxeSteel>())
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipePickaxeSteel>())
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipeToolboxT3>());
        }
    }
}