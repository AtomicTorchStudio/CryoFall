namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;

    public class AchievementDestroyPragmiumSource : ProtoAchievement
    {
        public override string AchievementId => "destroy_pragmium_source";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskDestroy.Require<ObjectMineralPragmiumSource>());
        }
    }
}