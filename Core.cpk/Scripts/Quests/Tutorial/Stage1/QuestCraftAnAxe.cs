namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data;

    public class QuestCraftAnAxe : ProtoQuest
    {
        public override string Description =>
            "Now that you have some resources, you can start crafting. First, craft a rope. Then, when it's done—craft an axe.";

        public const string HintHotkeys = "You can quickly open the crafting menu with the [b]\\[{0}\\][/b] key and inventory with the [b]\\[{1}\\][/b] key.";

        public override string Name => "Craft an axe";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskCraftRecipe.RequireHandRecipe<RecipeRope>())
                .Add(TaskCraftRecipe.RequireHandRecipe<RecipeAxeStone>());

            prerequisites
                .Add<QuestGatherResources>();

            hints
                .Add(() =>
                {
                    var keyForButton1 = InputKeyNameHelper.GetKeyText(ClientInputManager.GetKeyForButton(GameButton.CraftingMenu));
                    var keyForButton2 = InputKeyNameHelper.GetKeyText(ClientInputManager.GetKeyForButton(GameButton.InventoryMenu));

                    return string.Format(HintHotkeys, keyForButton1, keyForButton2); 
                });
        }
    }
}