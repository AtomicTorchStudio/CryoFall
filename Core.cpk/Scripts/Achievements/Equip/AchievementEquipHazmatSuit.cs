namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEquipHazmatSuit : ProtoAchievement
    {
        public override string AchievementId => "equip_hazmat_suit";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskHaveItemEquipped.Require<ItemHazmatSuit>());
        }
    }
}