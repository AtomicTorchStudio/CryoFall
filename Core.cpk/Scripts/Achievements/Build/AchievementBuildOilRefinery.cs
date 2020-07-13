namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class AchievementBuildOilRefinery : ProtoAchievement
    {
        public override string AchievementId => "build_oil_refinery";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskBuildStructure.Require<ObjectOilRefinery>());
        }
    }
}