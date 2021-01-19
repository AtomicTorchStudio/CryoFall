namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data;

    public class QuestPerformBasicActions : ProtoQuest
    {
        public const string HintCompletionist =
            "When you discover something new it will appear in your [b]completionist menu[/b]! Use it to track your progress and receive [u]additional LP[/u] for learning more about the world of CryoFall.";

        public const string HintSortInventory =
            "To sort your inventory, simply open it and press [u]{0}[/u] or [u]{1}[/u] key while hovering over your inventory grid. You can sort [b]ANY[/b] inventory grid like that (e.g. chests).";

        public const string HintSprint =
            "To sprint (run) you can simply hold [u]Shift key[/u] while moving. Run for a few meters to give it a try. Sprinting can help you get away from dangerous predators.";

        public override string Description =>
            "There are a few more things you need to know to fully enjoy CryoFall, like how to sort any inventory, and how to make your character sprint!";

        public override string Name => "Perform basic actions";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskSortItemsContainer.Require)
                .Add(TaskRun.Require)
                .Add(TaskCompleteCompletionistAnyEntry.Require);

            prerequisites
                .Add<QuestUnlockAndBuildWorkbench>();

            hints
                .Add(() =>
                     {
                         var keyForButton = ClientInputManager.GetMappingForAbstractButton(
                             WrappedButton<GameButton>.GetWrappedButton(GameButton.ContainerSort));
                         return string.Format(HintSortInventory,
                                              InputKeyNameHelper.GetKeyText(keyForButton.PrimaryKey),
                                              InputKeyNameHelper.GetKeyText(keyForButton.SecondaryKey));
                     })
                .Add(HintSprint)
                .Add(HintCompletionist);
        }
    }
}