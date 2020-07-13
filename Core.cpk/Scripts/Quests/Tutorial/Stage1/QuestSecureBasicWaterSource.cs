namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes;

    public class QuestSecureBasicWaterSource : ProtoQuest
    {
        public const string TaskGatherWaterbulbFruit = "Gather waterbulb fruit";

        public override string Description =>
            "It is time to learn how to secure a basic water source. When this planet was terraformed, one of the very first plants to be cultivated was waterbulb berry bushes. They provide a good source of clean and easy-to-access water.";

        public override string Hints =>
            @"[*] Waterbulb plants grow in almost all biomes, but they can be most commonly found growing on [b]meadows in temperate biomes[/b].
              [*] As with any plants and food in general they will spoil quickly, so they should be picked and consumed when you need water, rather than stored for later.";

        public override string Name => "Secure basic water source";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskGather.Require<ObjectBushWaterbulb>(count: 3, TaskGatherWaterbulbFruit))
                .Add(TaskUseItem.Require<ItemWaterbulb>());

            prerequisites
                .Add<QuestCookAnyFood>();
        }
    }
}