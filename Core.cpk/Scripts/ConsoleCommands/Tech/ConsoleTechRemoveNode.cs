// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Tech
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleTechRemoveNode : BaseConsoleCommand
    {
        public override string Description => "Remove a tech node from a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "tech.removeNode";

        public string Execute(TechNode node, [CurrentCharacterIfNull] ICharacter player = null)
        {
            player.SharedGetTechnologies().ServerRemoveNode(node);
            return $"{player} tech node {node.Name} removed.";
        }
    }
}