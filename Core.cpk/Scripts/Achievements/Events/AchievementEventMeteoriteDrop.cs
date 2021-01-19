namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;

    public class AchievementEventMeteoriteDrop : ProtoAchievement
    {
        public override string AchievementId => "event_meteorite_drop";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskDestroy.Require<ObjectMeteorite>());
        }
    }
}