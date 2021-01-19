namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementMintCoins : ProtoAchievement
    {
        public override string AchievementId => "mint_coins";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskManufactureItem.Require<ItemCoinPenny>())
                .Add(TaskManufactureItem.Require<ItemCoinShiny>());
        }
    }
}