namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Defense;
    using AtomicTorch.CBND.GameApi.Scripting;

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
                // suggest cloth hat but require any head item
                .Add(RequirementHaveItemEquipped.Require<IProtoItemEquipmentHead>(
                         string.Format(RequirementHaveItemEquipped.DescriptionFormat,
                                       Api.GetProtoEntity<ItemClothHat>().Name)))
                // suggest cloth shirt but require any chest item
                .Add(RequirementHaveItemEquipped.Require<IProtoItemEquipmentChest>(
                         string.Format(RequirementHaveItemEquipped.DescriptionFormat,
                                       Api.GetProtoEntity<ItemClothShirt>().Name)))
                // suggest cloth pants but require any legs item
                .Add(RequirementHaveItemEquipped.Require<IProtoItemEquipmentLegs>(
                         string.Format(RequirementHaveItemEquipped.DescriptionFormat,
                                       Api.GetProtoEntity<ItemClothPants>().Name)));

            prerequisites
                .Add<QuestUnlockAndBuildWorkbench>();
        }
    }
}