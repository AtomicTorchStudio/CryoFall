namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerGetPvpScore : BaseConsoleCommand
    {
        public override string Description =>
            "Gets the PvP Score for a player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.getPvpScore";

        public string Execute([CurrentCharacterIfNull] ICharacter character)
        {
            var statistics = PlayerCharacter.GetPrivateState(character).Statistics;
            return $"{character} has PvP score {statistics.PvpScore:0.##}";
        }
    }
}