namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementCompletionistLoot : ProtoAchievement
    {
        public override string AchievementId => "completionist_all_loot";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskCompleteCompletionistPage.Require(TaskCompleteCompletionistPage.CompletionistPageName.Loot));
        }
    }
}