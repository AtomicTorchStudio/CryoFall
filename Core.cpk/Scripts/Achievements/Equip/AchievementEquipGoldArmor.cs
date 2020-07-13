namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEquipGoldArmor : ProtoAchievement
    {
        public override string AchievementId => "equip_gold_armor";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskHaveItemEquipped.Require<ItemGoldArmor>());
            tasks.Add(TaskHaveItemEquipped.Require<ItemGoldHelmet>());
        }
    }
}