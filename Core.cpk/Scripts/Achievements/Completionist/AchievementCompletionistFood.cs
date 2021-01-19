namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementCompletionistFood : ProtoAchievement
    {
        public override string AchievementId => "completionist_all_food";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskCompleteCompletionistPage.Require(TaskCompleteCompletionistPage.CompletionistPageName.Food));
        }
    }
}