namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;

    public class QuestCraftAndEquipBetterArmor : ProtoQuest
    {
        public override string Description =>
            "It is time to get some better protection.";

        public override string Hints =>
            @"[*] You can unlock all relevant technologies in the ""Offense & Defense"" technology group.
              [*] Wooden armor may not be the best option out there, but it's still cheap to craft and certainly much better than cloth armor.";

        public override string Name => "Craft and equip better armor";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementBuildStructure.Require<ObjectArmorerWorkbench>())
                .Add(RequirementHaveItemEquipped.Require<ItemWoodHelmet>())
                .Add(RequirementHaveItemEquipped.Require<ItemWoodChestplate>())
                .Add(RequirementHaveItemEquipped.Require<ItemWoodPants>());

            prerequisites
                .Add<QuestClaySandGlassBottlesWaterCollector>();
        }
    }
}