namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class AchievementBuildMineralProcessingPlant : ProtoAchievement
    {
        public override string AchievementId => "build_mineral_processing_plant";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskBuildStructure.Require<ObjectMineralProcessingPlant>());
        }
    }
}