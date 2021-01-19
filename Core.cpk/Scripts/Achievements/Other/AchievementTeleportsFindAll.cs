namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementTeleportsFindAll : ProtoAchievement
    {
        public override string AchievementId => "teleports_find_all";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskDiscoverTeleportsAll.Require());
        }
    }
}