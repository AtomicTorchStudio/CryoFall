namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class QuestCraftAndEquipBetterArmor : ProtoQuest
    {
        public override string Description =>
            "It is time to get some better protection.";

        public override string Hints =>
            @"[*] You can unlock all relevant technologies in the ""Defense"" technology group.
              [*] Wooden armor may not be the best option out there, but it's still cheap to craft and certainly much better than cloth armor.";

        public override string Name => "Craft and equip better armor";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            var headEquipmentExceptCloth = Api.FindProtoEntities<IProtoItemEquipmentHead>()
                                              .Where(i => !(i is ItemClothHat))
                                              .ToList();

            var chestEquipmentExceptCloth = Api.FindProtoEntities<IProtoItemEquipmentChest>()
                                               .Where(i => !(i is ItemClothShirt))
                                               .ToList();

            var legsEquipmentExceptCloth = Api.FindProtoEntities<IProtoItemEquipmentLegs>()
                                              .Where(i => !(i is ItemClothPants))
                                              .ToList();
            requirements
                .Add(RequirementBuildStructure.Require<ObjectArmorerWorkbench>())
                // suggest wood helmet but require any head item except the cloth one
                .Add(RequirementHaveItemEquipped.Require(
                         headEquipmentExceptCloth,
                         string.Format(RequirementHaveItemEquipped.DescriptionFormat,
                                       Api.GetProtoEntity<ItemWoodHelmet>().Name)))
                // suggest wood chestplate but require any chest item except the cloth one
                .Add(RequirementHaveItemEquipped.Require(
                         chestEquipmentExceptCloth,
                         string.Format(RequirementHaveItemEquipped.DescriptionFormat,
                                       Api.GetProtoEntity<ItemWoodChestplate>().Name)))
                // suggest wood pants but require any legs item except the cloth one
                .Add(RequirementHaveItemEquipped.Require(
                         legsEquipmentExceptCloth,
                         string.Format(RequirementHaveItemEquipped.DescriptionFormat,
                                       Api.GetProtoEntity<ItemWoodPants>().Name)));

            prerequisites
                .Add<QuestExploreBiomes1>();
        }
    }
}