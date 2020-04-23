namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class AchievementBuildVehicleAssemblyBay : ProtoAchievement
    {
        public override string AchievementId => "build_vehicle_assembly_bay";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskBuildStructure.Require<ObjectVehicleAssemblyBay>());
        }
    }
}