namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementCompletionistCreatures : ProtoAchievement
    {
        public override string AchievementId => "completionist_all_creatures";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskCompleteCompletionistPage.Require(TaskCompleteCompletionistPage.CompletionistPageName.Creatures));
        }
    }
}