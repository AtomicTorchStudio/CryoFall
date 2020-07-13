namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementUseStimpack : ProtoAchievement
    {
        public override string AchievementId => "use_stimpack";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskCraftRecipe.RequireStationRecipe<RecipeStimpack>());
            tasks.Add(TaskUseItem.Require<ItemStimpack>());
        }
    }
}