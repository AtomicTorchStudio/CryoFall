namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Defense;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Offense;

    public class QuestCompleteTier1Technologies : ProtoQuest
    {
        public override string Description =>
            "Higher level technologies require mastery of early tech to progress further.";

        public override string Hints =>
            "[*] You can unlock higher tiers of technological groups to gain access to more complex technologies, recipes and structures.";

        public override string Name => "Mastering technologies—part one";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskCompleteTechGroup.Require<TechGroupConstructionT1>())
                .Add(TaskCompleteTechGroup.Require<TechGroupIndustryT1>())
                .Add(TaskCompleteTechGroup.Require<TechGroupFarmingT1>())
                .Add(TaskCompleteTechGroup.Require<TechGroupCookingT1>())
                .Add(TaskCompleteTechGroup.Require<TechGroupOffenseT1>())
                .Add(TaskCompleteTechGroup.Require<TechGroupDefenseT1>());

            prerequisites
                .Add<QuestExploreBiomes2>()
                .Add<QuestMasterHunter2>()
                .Add<QuestBuildALampCraftCampfuel>();
        }
    }
}