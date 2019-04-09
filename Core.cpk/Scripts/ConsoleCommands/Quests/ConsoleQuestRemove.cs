namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Quests
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleQuestRemove : BaseConsoleCommand
    {
        public override string Description => "Remove quest from a player (if exist).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "quest.remove";

        public string Execute(IProtoQuest quest, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var quests = player.SharedGetQuests();
            quests.ServerTryRemoveQuest(quest);
            quests.ServerTryAddQuest(quest, isUnlocked: false);
            return null;
        }
    }
}