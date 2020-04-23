namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables;

    public class AchievementCollectHerbs : ProtoAchievement
    {
        public override string AchievementId => "collect_herbs";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskGather.Require<ObjectSmallHerbGreen>())
                .Add(TaskGather.Require<ObjectSmallHerbRed>())
                .Add(TaskGather.Require<ObjectSmallHerbPurple>())
                .Add(TaskGather.Require<ObjectSmallHerbBlue>());
        }
    }
}