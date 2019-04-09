namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    public class QuestPerformBasicActions : ProtoQuest
    {
        public override string Description =>
            "There are a few more things you need to know to fully enjoy CryoFall, like how to sort any inventory, and how to make your character sprint!";

        public override string Hints =>
            @"[*] To sort your inventory, simply open it and press [u]middle mouse button[/u] or [u]Z key[/u] (if you haven't changed the controls) while hovering over your inventory grid. You can sort [b]ANY[/b] inventory grid like that (e.g. chests).
              [*] To sprint (run) you can simply hold [u]Shift key[/u] while moving. Run for a few meters to give it a try. Sprinting can help you get away from dangerous predators.";

        public override string Name => "Perform basic actions";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementSortItemsContainer.Require)
                .Add(RequirementRun.Require);

            prerequisites
                .Add<QuestChopDownATree>();
        }
    }
}