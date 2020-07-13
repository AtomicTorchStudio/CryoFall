namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEquipRespirator : ProtoAchievement
    {
        public override string AchievementId => "equip_respirator";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskHaveItemEquipped.Require<ItemHelmetRespirator>());
        }
    }
}