namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class AchievementBuildOilExtractor : ProtoAchievement
    {
        public override string AchievementId => "build_oil_extractor";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskBuildStructure.Require<ObjectOilPumpAdvanced>());
        }
    }
}