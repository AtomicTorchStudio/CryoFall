namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;

    public class QuestMasterHunter2 : ProtoQuest
    {
        public override string Description =>
            "Time for some more hunting! There are still more animals and monsters to find.";

        public override string Hints =>
            @"[*] Each animal and monster has their own area where they can normally be found. Try to explore the world and see all the different creatures that inhabit it.
              [*] Depending on how dangerous a particular animal or monster is, you will get different amounts of hunting experience.";

        public override string Name => "Master hunter—part two";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementKill.Require<MobPangolin>(count: 1))
                .Add(RequirementKill.Require<MobTropicalBoar>(count: 1))
                .Add(RequirementKill.Require<MobTurtle>(count: 1))
                .Add(RequirementKill.Require<MobSnakeGreen>(count: 1));

            prerequisites
                .Add<QuestMasterHunter1>();
        }
    }
}