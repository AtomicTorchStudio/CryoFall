namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class QuestFindAndUseGreenHerb : ProtoQuest
    {
        public override string Description =>
            "There are much better medical items available out there, but in a pinch you can just use some green herbs to restore a little bit of health.";

        public override string Hints =>
            @"[*] Green herb is the most basic of all types of medicine, and it's relatively easy to obtain.
              [*] You can typically find herbs and other small plants growing in vast numbers in meadows in the forest.
              [*] If you have a lot of green herbs cluttering your chests you can use them as-is, but otherwise it is wise to use them to craft proper medical items.";

        public override string Name => "Find and use green herb";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            var iconHerbGreen = Api.IsClient
                                    ? Api.GetProtoEntity<ObjectSmallHerbGreen>().Icon
                                    : null;

            tasks
                .Add(TaskHaveItem.Require<ItemHerbGreen>(count: 1, isReversible: false)
                                 .WithIcon(iconHerbGreen))
                .Add(TaskUseItem.Require<ItemHerbGreen>(count: 1));

            prerequisites
                .Add<QuestCraftAKnifeAndKillAnyCreature>();
        }
    }
}