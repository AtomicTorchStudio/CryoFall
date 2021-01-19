namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class QuestExploreRuins : ProtoQuest
    {
        public const string TaskGatherLoot = "Loot some containers";

        public override string Description =>
            "Old ruins are the best place to find rare components and certain resources. It is always a good idea to explore them occasionally, especially if you have the necessary gear.";

        public override string Hints =>
            @"[*] There are different types of ruins: residential (T1), industrial (T2), military (T2) and scientific (T3).
              [*] Difficulty of the ruins ranges from T1, which can be explored by anyone, to T3, which requires the best equipment and meds in CryoFall.
              [*] Exploring ruins is the only way to obtain certain rare items. Chance to find them depends on difficulty of the ruins, and the most valuable items can be found only in T3 ruins.";

        public override string Name => "Explore old ruins";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskHaveItemEquipped.Require<ItemHelmetRespirator>())
                .Add(TaskVisitTile.Require<TileRuins>())
                .Add(TaskGather.Require(
                                   // All loot containers but not loot piles (IsAutoTakeAll => false)
                                   Api.FindProtoEntities<ProtoObjectLootContainer>()
                                      .Where(p => !p.IsAutoTakeAll)
                                      .ToList(),
                                   count: 5,
                                   TaskGatherLoot)
                               .WithIcon(Api.GetProtoEntity<ObjectLootCrateIndustrial>().Icon));

            prerequisites
                .Add<QuestCompleteTier1Technologies>();
        }
    }
}