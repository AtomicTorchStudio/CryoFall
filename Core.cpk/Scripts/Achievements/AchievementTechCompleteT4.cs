namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Technologies;

    public class AchievementTechCompleteT4 : ProtoAchievement
    {
        public override string AchievementId => "tech_complete_t4";

        protected override void PrepareAchievement(TasksList tasks)
        {
            foreach (var protoTechGroup in TechGroup.AvailableTechGroups)
            {
                if (protoTechGroup.Tier == TechTier.Tier4)
                {
                    tasks.Add(TaskCompleteTechGroup.Require(protoTechGroup));
                }
            }
        }
    }
}