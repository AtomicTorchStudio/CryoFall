namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementUseDrones : ProtoAchievement
    {
        public override string AchievementId => "use_drones";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskUseItem.Require<IProtoItemDroneControl>());
        }
    }
}