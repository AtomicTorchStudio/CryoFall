namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;

    public class QuestUseCrowbarAndDeconstructBuilding : ProtoQuest
    {
        public override string Description =>
            "Being able to build anything is nice, but you might also want to know how to deconstruct your buildings. To do that, you can use a crowbar! As long as the building you are trying to deconstruct is inside of your land claim area, that is.";

        public override string Hints =>
            @"[*] For the purposes of this quest you can deconstruct a section of the wall or something equally cheap, so you don't lose valuable resources.
              [*] You can [b]only[/b] deconstruct buildings [b]inside[/b] your land claim area.
              [*] Deconstruction is the best way to remove any unnecessary or obsolete building or structure.";

        public override string Name => "Use crowbar for deconstruction";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks)
        {
            tasks
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipeCrowbar>())
                .Add(TaskDeconstructStructure.Require<IProtoObjectStructure>(
                         description: this.Name));

            prerequisites
                .Add<QuestCraftIronTools>();
        }
    }
}