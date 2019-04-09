namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Quests
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleQuestCompleteAll : BaseConsoleCommand
    {
        public override string Description =>
            "Complete all quests to a player (even if the quest is not added or the prerequisites are not satisfied).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "quest.completeAll";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var quests = player.SharedGetQuests();
            foreach (var quest in QuestsSystem.AllQuests)
            {
                quests.ServerTryAddQuest(quest, isUnlocked: true);
                if (!quests.SharedHasCompletedQuest(quest))
                {
                    QuestsSystem.ServerCompleteQuest(quests, quest, ignoreRequirements: true);
                }
            }

            return null;
        }
    }
}