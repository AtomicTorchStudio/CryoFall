namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;

    public class AchievementHarvestAllCrops : ProtoAchievement
    {
        public override string AchievementId => "harvest_all_crops";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskGather.Require<IProtoObjectPlant>()); // all farm plants
        }
    }
}