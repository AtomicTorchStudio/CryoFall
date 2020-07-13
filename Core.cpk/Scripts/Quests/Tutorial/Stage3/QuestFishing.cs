namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class QuestFishing : ProtoQuest
    {
        public const string HintFillet =
            "Fish [u]fillet[/u] can be [u]extracted[/u] from the caught fish with the [b]use[/b] action (i.e., the same as using any item).";

        public const string HintSkill =
            "Your [b]fishing skill[/b] plays a big role in what types of fish you can catch and your overall success.";

        public const string HintWaterTypes =
            "You can catch different fish in [u]salty[/u] and [u]freshwater[/u] bodies.";

        public const string TaskCatchAnyFish = "Catch any fish";

        public const string TaskCutAnyFish = "Cut any fish into fillet";

        public override string Description =>
            "Fishing is not only a good pastime, but also a great way to get more food. Try catching some fish!";

        public override string Name => "Fishing!";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage4;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipeFishingRod>())
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipeFishingBaitInsects>())
                .Add(TaskCatchFish.Require<IProtoItemFish>(count: 3, description: TaskCatchAnyFish))
                .Add(TaskUseItem.Require<IProtoItemFish>(count: 3, description: TaskCutAnyFish));

            prerequisites
                .Add<QuestEstablishPowerGrid>();

            hints
                .Add(HintFillet)
                .Add(HintWaterTypes)
                .Add(HintSkill);
        }
    }
}