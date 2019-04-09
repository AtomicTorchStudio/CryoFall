namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Quests
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleQuestRemoveAll : BaseConsoleCommand
    {
        public override string Description => "Remove all quests from a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "quest.removeAll";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var quests = player.SharedGetQuests();
            quests.ServerReset();
            return null;
        }
    }
}