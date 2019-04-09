namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Tech
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleTechRemoveAll : BaseConsoleCommand
    {
        public override string Description => "Remove all tech groups/nodes from a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "tech.removeAll";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var technologies = player.SharedGetTechnologies();
            technologies.ServerReset();
            return null;
        }
    }
}