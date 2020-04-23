namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class QuestClaySandGlassBottlesWaterCollector : ProtoQuest
    {
        public override string Description =>
            "To survive and thrive you need easy access to large quantities of water. Not just for drinking, but also for industrial purposes.";

        public override string Hints =>
            @"[*] You can easily find a virtually unlimited amount of [b]sand[/b] on any [b]beach or shore[/b].
              [*] You can find [b]ash[/b] after burning wood and especially twigs.
              [*] Water is used in many different crafting recipes, so you should always store a good amount for when you need it.
              [*] Rather than using a water collector, you can also boil lake water to purify it and make it ready for drinking or for use in crafting.
              [*] Wells are much more effective than simple water collectors.
              [*] You can boil salty ocean water to extract salt.";

        public override string Name => "Secure better water source";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks)
        {
            tasks
                .Add(TaskBuildStructure.Require<ObjectWaterCollector>())
                .Add(TaskManufactureItem.Require<ItemGlassRaw>(count: 50))
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipeBottle>(count: 5));

            prerequisites
                .Add<QuestUnlockSkills>()
                .Add<QuestBuildFurnaceAndSmeltCopper>();
        }
    }
}