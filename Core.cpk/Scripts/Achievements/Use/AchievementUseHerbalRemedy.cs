namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementUseHerbalRemedy : ProtoAchievement
    {
        public override string AchievementId => "use_herbal_remedy";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskCraftRecipe.RequireStationRecipe<RecipeHerbalRemedy>());
            tasks.Add(TaskUseItem.Require<ItemRemedyHerbal>());
        }
    }
}