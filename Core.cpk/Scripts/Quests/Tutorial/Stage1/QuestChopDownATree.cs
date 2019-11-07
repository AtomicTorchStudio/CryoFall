namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;

    public class QuestChopDownATree : ProtoQuest
    {
        public override string Description =>
            "It is time to get some wood! Find a tree and use your axe to chop it down.";

        public override string Hints =>
            @"[*] You need to place the axe into your [b]hotbar[/b]. Open the inventory and drag the axe you crafted into any hotbar slot below, then close your inventory.
              [*] Now you can [b]select[/b] it by pressing the corresponding number button or clicking it with your mouse (when your inventory is closed).
              [*] By default [b]left mouse button[/b] always uses the item you are holding, while [b]right mouse button[/b] interacts with an object in the world.";

        public override string Name => "Chop down a tree";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementDestroy.Require<IProtoObjectTree>(count: 3, description: this.Name));

            prerequisites
                .Add<QuestCraftAnAxe>();
        }
    }
}