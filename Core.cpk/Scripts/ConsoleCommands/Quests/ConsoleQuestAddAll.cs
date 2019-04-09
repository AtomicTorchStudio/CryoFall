namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Quests
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleQuestAddAll : BaseConsoleCommand
    {
        public override string Description => "Add all quests to a player (even if prerequisites are not satisfied).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "quest.addAll";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var quests = player.SharedGetQuests();
            foreach (var quest in QuestsSystem.AllQuests)
            {
                quests.ServerTryAddQuest(quest, isUnlocked: true);
            }

            return null;
        }
    }
}