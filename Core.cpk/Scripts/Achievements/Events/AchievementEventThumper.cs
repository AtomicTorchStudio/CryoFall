namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEventThumper : ProtoAchievement
    {
        public override string AchievementId => "event_thumper";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskCompleteCompletionistEntry.Require<MobThumper>());
        }
    }
}