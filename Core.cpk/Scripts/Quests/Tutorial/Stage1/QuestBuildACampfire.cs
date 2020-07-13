namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class QuestBuildACampfire : ProtoQuest
    {
        public override string Description =>
            "Now that you have a toolbox, you can start building the different structures you have unlocked in the technology menu. Let's start with a campfire, which can be used to cook food.";

        public override string Hints =>
            @"[*] You can press ""Tab"" to quickly access the [b]build menu[/b] if you haven't changed the controls.
              [*] The build menu may have [b]several pages[/b] with different [b]building types[/b] as you unlock more structures.
              [*] You can use toolboxes not only to [b]build[/b] structures, but also to [b]repair[/b] them.";

        public override string Name => "Build a campfire";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskBuildStructure.Require<ObjectCampfire>());

            prerequisites
                .Add<QuestCraftAToolbox>();
        }
    }
}