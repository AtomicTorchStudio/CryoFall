// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Tech
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleTechAddNode : BaseConsoleCommand
    {
        public override string Description =>
            "Add a tech node to a player. It will also add all other required nodes if there's a conflict.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "tech.addNode";

        public string Execute(TechNode node, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var technologies = player.SharedGetTechnologies();
            technologies.ServerAddNode(node);
            return $"{player} tech node {node.Name} added.";
        }
    }
}