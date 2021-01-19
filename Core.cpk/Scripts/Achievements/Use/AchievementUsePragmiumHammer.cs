namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementUsePragmiumHammer : ProtoAchievement
    {
        public override string AchievementId => "use_pragmium_hatchet";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskCraftRecipe.RequireStationRecipe<RecipeHammerPragmium>());
            tasks.Add(TaskUseItem.Require<ItemHammerPragmium>());
        }
    }
}