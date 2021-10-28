namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerSetPvpScore : BaseConsoleCommand
    {
        public override string Description =>
            "Sets PvP Score to a player character. It's available only in debug server mode.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setPvpScore";

        public string Execute(double score, [CurrentCharacterIfNull] ICharacter character)
        {
            var statistics = PlayerCharacter.GetPrivateState(character).Statistics;
            statistics.PvpScore = score;
            return $"{character} score set to {statistics.PvpScore:0.##}";
        }
    }
}