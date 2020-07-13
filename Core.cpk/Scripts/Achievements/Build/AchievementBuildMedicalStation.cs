namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class AchievementBuildMedicalStation : ProtoAchievement
    {
        public override string AchievementId => "build_medical_station";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskBuildStructure.Require<ObjectMedicalStation>());
        }
    }
}