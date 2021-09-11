namespace AtomicTorch.CBND.CoreMod.Achievements
{
    public class AchievementCompleteTheGame : ProtoAchievement
    {
        public override string AchievementId => "complete_the_game";

        protected override void PrepareAchievement(TasksList tasks)
        {
            // see ObjectLaunchpadStage5 where the achievement is added
        }
    }
}