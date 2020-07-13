namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class QuestCraftAndEquipBetterArmor : ProtoQuest
    {
        public const string EquipAnyArmor = "Equip better armor";

        public const string EquipAnyHelmet = "Equip better helmet";

        public override string Description =>
            "It is time to get some better protection.";

        public override string Hints =>
            @"[*] You can unlock all relevant technologies in the ""Defense"" technology group.
              [*] Wooden armor may not be the best option out there, but it's still cheap to craft and certainly much better than cloth armor.";

        public override string Name => "Craft and equip better armor";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            var headEquipmentExceptCloth = Api.FindProtoEntities<IProtoItemEquipmentHead>()
                                              .Where(i => !(i is ItemClothHelmet))
                                              .ToList();

            var chestEquipmentExceptCloth = Api.FindProtoEntities<IProtoItemEquipmentArmor>()
                                               .Where(i => !(i is ItemClothArmor))
                                               .ToList();

            tasks
                .Add(TaskBuildStructure.Require<ObjectArmorerWorkbench>())
                // suggest wood helmet but require any head item except the cloth one
                .Add(TaskHaveItemEquipped.Require(
                         headEquipmentExceptCloth,
                         EquipAnyHelmet))
                // suggest wood armor but require any armor item except the cloth one
                .Add(TaskHaveItemEquipped.Require(
                         chestEquipmentExceptCloth,
                         EquipAnyArmor));

            prerequisites
                .Add<QuestExploreBiomes1>();
        }
    }
}