namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense;

    public class QuestCraftAndEquipClothArmor : ProtoQuest
    {
        public override string Description =>
            "Running around without any protection would certainly lead to a quick death. Having even a basic armor is always a good idea. Research, craft and equip basic cloth armor.";

        public override string Hints =>
            @"[*] Cloth armor may not provide significant protection, but it's cheap to craft and certainly better than nothing.
              [*] There are [b]different damage types[/b], and different armor offers different protection values for each. You can see this information in your inventory screen.";

        public override string Name => "Craft and equip cloth armor";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementHaveTechNode.Require<TechNodeClothArmor>())
                .Add(RequirementHaveItemEquipped.Require<ItemClothHat>())
                .Add(RequirementHaveItemEquipped.Require<ItemClothShirt>())
                .Add(RequirementHaveItemEquipped.Require<ItemClothPants>());

            prerequisites
                .Add<QuestUnlockAndBuildWorkbench>();
        }
    }
}