namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEventSpaceDrop : ProtoAchievement
    {
        public override string AchievementId => "event_space_drop";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskCompleteCompletionistEntry.Require<EventSpaceDrop>());
        }
    }
}