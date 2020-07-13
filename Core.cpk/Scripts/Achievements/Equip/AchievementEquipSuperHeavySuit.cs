namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment.SuperHeavyArmor;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEquipSuperHeavySuit : ProtoAchievement
    {
        public override string AchievementId => "equip_superheavy_suit";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskHaveItemEquipped.Require<ItemSuperHeavySuit>());
        }
    }
}