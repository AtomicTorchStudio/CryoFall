// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Tech
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleTechAddAll : BaseConsoleCommand
    {
        public override string Description => "Add all tech groups and all nodes to a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "tech.addAll";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var technologies = player.SharedGetTechnologies();
            foreach (var group in Api.FindProtoEntities<TechGroup>())
            {
                // add tech group
                technologies.ServerAddGroup(group);

                // add all nodes
                foreach (var techNode in group.Nodes)
                {
                    technologies.ServerAddNode(techNode);
                }
            }

            return $"{player} all tech groups and nodes added.";
        }
    }
}