namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;

    public class AchievementEventSpaceDrop : ProtoAchievement
    {
        public override string AchievementId => "event_space_drop";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskUseObject.Require<ObjectSpaceDebris>());
        }
    }
}