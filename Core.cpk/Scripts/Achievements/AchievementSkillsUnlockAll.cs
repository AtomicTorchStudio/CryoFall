namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class AchievementSkillsUnlockAll : ProtoAchievement
    {
        public override string AchievementId => "skills_unlock_all";

        protected override void PrepareAchievement(TasksList tasks)
        {
            foreach (var protoSkill in Api.FindProtoEntities<IProtoSkill>())
            {
                tasks.Add(TaskHaveSkill.Require(protoSkill, minLevel: 1));
            }
        }
    }
}