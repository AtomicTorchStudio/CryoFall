namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;

    public class AchievementCompletionistEvents : ProtoAchievement
    {
        public override string AchievementId => "completionist_all_events";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskCompleteCompletionistPage.Require(CompletionistPageName.Events));
        }
    }
}