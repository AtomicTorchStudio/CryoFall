namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;

    public class QuestLearnBasicBuilding : ProtoQuest
    {
        public override string Description =>
            @"To get access to more structures, you need to unlock ""basic buildings"" in the technology menu.";

        public override string Hints =>
            @"[*] You can quickly access the technologies menu by pressing ""G"" if you haven't changed the controls.
              [*] Unlocking any technology node requires [b]LP[/b] (learning points).
              [*] Learning points are earned from any meaningful activity and are directly proportional to your skills experience gain.";

        public override string Name => "Learn basic building technology";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks)
        {
            tasks
                .Add(TaskHaveTechNode.Require<TechNodeBasicBuilding>())
                .Add(TaskHaveTechNode.Require<TechNodeWoodDoor>())
                .Add(TaskBuildStructure.Require<ObjectWallWood>())
                .Add(TaskBuildStructure.Require<ObjectDoorWood>());

            prerequisites
                .Add<QuestBuildAPermanentBase>();
        }
    }
}