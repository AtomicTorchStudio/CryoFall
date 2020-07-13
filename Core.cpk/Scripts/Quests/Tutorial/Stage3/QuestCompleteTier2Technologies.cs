namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Electricity;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Farming;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Fishing;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Medicine;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense;

    public class QuestCompleteTier2Technologies : ProtoQuest
    {
        public override string Description =>
            "At this point it might be a good idea to complete all of the Tier 2 technologies, to have a solid foundation for further specialization in a particular area.";

        public override string Hints =>
            @"[*] Early technologies (Tier 1 and 2) are relatively cheap to unlock, so it is always a good idea to learn all of them.
              [*] For advanced technologies (Tier 3 and up), it is a good idea to specialize in just a few. Consider which of them are most important to you, rather then trying to learn all of them.";

        public override string Name => "Mastering technologies—part two";

        // TODO: revert this back to normal constant for the given level. For now it is set to 250 until we have a decent solution for this quest.
        public override ushort RewardLearningPoints => 250;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskCompleteTechGroup.Require<TechGroupChemistryT2>())
                .Add(TaskCompleteTechGroup.Require<TechGroupConstructionT2>())
                .Add(TaskCompleteTechGroup.Require<TechGroupCookingT2>())
                .Add(TaskCompleteTechGroup.Require<TechGroupDefenseT2>())
                .Add(TaskCompleteTechGroup.Require<TechGroupFarmingT2>())
                .Add(TaskCompleteTechGroup.Require<TechGroupFishingT2>())
                .Add(TaskCompleteTechGroup.Require<TechGroupIndustryT2>())
                .Add(TaskCompleteTechGroup.Require<TechGroupElectricityT2>())
                .Add(TaskCompleteTechGroup.Require<TechGroupMedicineT2>())
                .Add(TaskCompleteTechGroup.Require<TechGroupOffenseT2>());

            prerequisites
                .Add<QuestMasterHunter4>()
                .Add<QuestExploreBiomes4>();
        }
    }
}