namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class QuestExploreRuins : ProtoQuest
    {
        public override string Description =>
            "Old ruins are the best place to find rare components and certain resources. It is always a good idea to explore them occasionally, especially if you have the necessary gear.";

        public override string Hints =>
            @"[*] There are different types of ruins: residential (T1), industrial (T2), military (T2) and scientific (T3).
              [*] Difficulty of the ruins ranges from T1, which can be explored by anyone, to T3, which requires the best equipment and meds in CryoFall.
              [*] Exploring ruins is the only way to obtain certain rare items. Chance to find them depends on difficulty of the ruins, and the most valuable items can be found only in T3 ruins.";

        public override string Name => "Explore old ruins";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        public const string TaskGatherLoot = "Loot some containers";

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks)
        {
            tasks
                .Add(TaskHaveItemEquipped.Require<ItemRespirator>())
                .Add(TaskVisitTile.Require<TileRuins>())
                .Add(TaskGather.Require(new IProtoObjectGatherable[]
                                        {
                                            Api.GetProtoEntity<ObjectLootCrateFood>(),
                                            Api.GetProtoEntity<ObjectLootCrateHightech>(),
                                            Api.GetProtoEntity<ObjectLootCrateIndustrial>(),
                                            Api.GetProtoEntity<ObjectLootCrateMedical>(),
                                            Api.GetProtoEntity<ObjectLootCrateMilitary>(),
                                            Api.GetProtoEntity<ObjectLootCrateSupply>()
                                        },
                                        count: 5,
                                        TaskGatherLoot));

            prerequisites
                .Add<QuestCompleteTier1Technologies>();
        }
    }
}