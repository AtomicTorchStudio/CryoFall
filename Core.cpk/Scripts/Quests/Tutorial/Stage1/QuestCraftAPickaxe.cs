namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data;

    public class QuestCraftAPickaxe : ProtoQuest
    {
        public override string Description =>
            "Mining different minerals offers access to the most valuable resources in CryoFall. You need to craft a pickaxe to be able to mine those minerals.";

        public const string HintCrafting = "Open your handcrafting menu ([b]\\[{0}\\][/b] key) and craft a pickaxe.";
        public const string HintRope = "You may need to craft some rope if you don't have any.";

        public override string Name => "Craft a pickaxe";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskCraftRecipe.RequireHandRecipe<RecipePickaxeStone>());

            prerequisites
                .Add<QuestChopDownATree>();

            hints
                .Add(() =>
                {
                    var keyForButton = InputKeyNameHelper.GetKeyText(ClientInputManager.GetKeyForButton(GameButton.CraftingMenu));
                    return string.Format(HintCrafting, keyForButton);
                })
                .Add(HintRope);
        }
    }
}