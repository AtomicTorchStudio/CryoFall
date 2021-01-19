namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Items.Fishing;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementEventBlueGlider : ProtoAchievement
    {
        public override string AchievementId => "event_blue_glider";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks
                .Add(TaskCatchFish.Require<ItemFishBlueGlider>());
        }
    }
}