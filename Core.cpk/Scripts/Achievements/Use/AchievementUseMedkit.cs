namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementUseMedkit : ProtoAchievement
    {
        public override string AchievementId => "use_medkit";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskCraftRecipe.RequireStationRecipe<RecipeMedkit>());
            tasks.Add(TaskUseItem.Require<ItemMedkit>());
        }
    }
}