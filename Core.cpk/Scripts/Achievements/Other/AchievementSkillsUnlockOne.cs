namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class AchievementSkillsUnlockOne : ProtoAchievement
    {
        public override string AchievementId => "skills_unlock_one";

        protected override void PrepareAchievement(TasksList tasks)
        {
            tasks.Add(TaskHaveSkills.RequireAny(count: 1, minLevel: 1));
        }
    }
}