namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEquipNightVisionAdvanced : ProtoAchievement
    {
        public override string AchievementId => "equip_night_vision_advanced";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskHaveItemEquipped.Require<ItemHelmetNightVisionAdvanced>());
        }
    }
}