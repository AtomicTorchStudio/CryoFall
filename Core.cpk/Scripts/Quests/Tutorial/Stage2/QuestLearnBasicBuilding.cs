namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data;

    public class QuestLearnBasicBuilding : ProtoQuest
    {
        public const string HintLPGain =
            "Learning points are earned from any meaningful activity and are directly proportional to your skills experience gain.";

        public const string HintTechMenu =
            "You can quickly access the technologies menu by pressing the [b]\\[{0}\\][/b] key.";

        public const string HintTechUnlock = "Unlocking any technology node requires [b]LP[/b] (learning points).";

        public override string Description =>
            @"To get access to more structures, you need to unlock ""basic buildings"" in the technology menu.";

        public override string Name => "Learn basic building technology";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskHaveTechNode.Require<TechNodeBasicBuilding>())
                .Add(TaskHaveTechNode.Require<TechNodeWoodDoor>())
                .Add(TaskBuildStructure.Require<ObjectWallWood>())
                .Add(TaskBuildStructure.Require<ObjectDoorWood>());

            prerequisites
                .Add<QuestBuildAPermanentBase>();

            hints
                .Add(() =>
                     {
                         var keyForButton =
                             InputKeyNameHelper.GetKeyText(
                                 ClientInputManager.GetKeyForButton(GameButton.TechnologiesMenu));
                         return string.Format(HintTechMenu, keyForButton);
                     })
                .Add(HintTechUnlock)
                .Add(HintLPGain);
        }
    }
}