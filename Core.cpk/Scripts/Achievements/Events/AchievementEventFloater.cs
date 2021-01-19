namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEventFloater : ProtoAchievement
    {
        public override string AchievementId => "event_floater";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskKill.Require<MobFloater>());
        }
    }
}