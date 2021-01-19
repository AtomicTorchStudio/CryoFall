namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class QuestDroneMining : ProtoQuest
    {
        public const string HintDroneControl =
            "Use [b]control device[/b] to issue commands to drones.";

        public const string HintDroneTasks =
            "Drones can be used not only to [b]mine minerals[/b] but to [b]fell trees[/b] as well.";

        public const string HintDroneUsage =
            "Drones must be in your [b]inventory[/b] or [b]hotbar[/b] to be issued commands.";

        public override string Description =>
            "Mining minerals and chopping trees by hand is tedious, so you should definitely switch to having automated drones do it for you as soon as you can.";

        public override string Name => "Drone mining";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage4;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            var recipeDroneControlStandard = Api.GetProtoEntity<RecipeDroneControlStandard>();
            var recipeDroneControlAdvanced = Api.GetProtoEntity<RecipeDroneControlAdvanced>();
            var recipeDroneIndustrialStandard = Api.GetProtoEntity<RecipeDroneIndustrialStandard>();
            var recipeDroneIndustrialAdvanced = Api.GetProtoEntity<RecipeDroneIndustrialAdvanced>();
            var protoItemRemoteControlStandard = Api.GetProtoEntity<ItemDroneControlStandard>();

            // require crafting any drone control
            tasks.Add(
                TaskCraftRecipe.RequireStationRecipe(
                                   new List<Recipe.RecipeForStationCrafting>()
                                   {
                                       recipeDroneControlStandard,
                                       recipeDroneControlAdvanced
                                   },
                                   count: 1,
                                   description: TaskCraftRecipe.AppendRecipeLocationIfNecessary(
                                       TaskCraftRecipe.DescriptionTitlePrefix + " " + recipeDroneControlStandard.Name,
                                       recipeDroneControlStandard))
                               .WithIcon(recipeDroneControlStandard.Icon));

            // require crafting any drone (an item)
            tasks.Add(
                TaskCraftRecipe.RequireStationRecipe(
                                   new List<Recipe.RecipeForStationCrafting>()
                                   {
                                       recipeDroneIndustrialStandard,
                                       recipeDroneIndustrialAdvanced
                                   },
                                   count: 1,
                                   description: TaskCraftRecipe.AppendRecipeLocationIfNecessary(
                                       TaskCraftRecipe.DescriptionTitlePrefix
                                       + " "
                                       + recipeDroneIndustrialStandard.Name,
                                       recipeDroneIndustrialStandard))
                               .WithIcon(recipeDroneIndustrialStandard.Icon));

            // require using any drone item
            tasks.Add(
                TaskUseItem.Require(
                               Api.FindProtoEntities<IProtoItemDroneControl>(),
                               count: 1,
                               description: string.Format(TaskUseItem.DescriptionFormat,
                                                          protoItemRemoteControlStandard.Name))
                           .WithIcon(Api.GetProtoEntity<ItemDroneControlStandard>().Icon));

            prerequisites
                .Add<QuestAdvancedResourcesAcquisition>();

            hints
                .Add(HintDroneTasks)
                .Add(HintDroneControl)
                .Add(HintDroneUsage);
        }
    }
}