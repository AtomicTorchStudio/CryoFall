namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class AchievementHarvestAllCrops : ProtoAchievement
    {
        public override string AchievementId => "harvest_all_crops";

        protected override void PrepareAchievement(TasksList tasks)
        {
            foreach (var protoPlant in Api.FindProtoEntities<IProtoObjectPlant>())
            {
                tasks.Add(TaskGather.Require(protoPlant));
            }
        }
    }
}