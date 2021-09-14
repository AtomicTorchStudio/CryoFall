namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;

    public class AchievementCompletionistFish : ProtoAchievement
    {
        public override string AchievementId => "completionist_all_fish";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskCompleteCompletionistPage.Require(CompletionistPageName.Fish));
        }
    }
}