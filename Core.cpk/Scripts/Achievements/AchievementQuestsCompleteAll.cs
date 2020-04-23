namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;

    public class AchievementQuestsCompleteAll : ProtoAchievement
    {
        public override string AchievementId => "quests_complete_all";

        protected override void PrepareAchievement(TasksList tasks)
        {
            foreach (var protoQuest in QuestsSystem.AllQuests)
            {
                tasks.Add(TaskCompleteQuest.Require(protoQuest));
            }
        }
    }
}