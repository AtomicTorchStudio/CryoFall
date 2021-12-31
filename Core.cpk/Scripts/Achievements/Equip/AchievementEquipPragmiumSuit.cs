namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEquipPragmiumSuit : ProtoAchievement
    {
        public override string AchievementId => "equip_pragmium_suit";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskHaveItemEquipped.Require<ItemPragmiumSuit>());
        }
    }
}