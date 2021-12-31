namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEquipApartSuit : ProtoAchievement
    {
        public override string AchievementId => "equip_apart_suit";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskHaveItemEquipped.Require<ItemApartSuit>());
        }
    }
}