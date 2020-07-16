// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Tech
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleTechAddTier : BaseConsoleCommand
    {
        public override string Description =>
            "Add a particular tech tier to a player. The argument controls whether the tech nodes of the tech gr";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "tech.addTier";

        public string Execute(TechTier tier, bool withNodes, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var technologies = player.SharedGetTechnologies();

            foreach (var techGroup in TechGroup.AvailableTechGroups)
            {
                if (techGroup.Tier != tier)
                {
                    continue;
                }

                technologies.ServerAddGroup(techGroup);

                if (!withNodes)
                {
                    continue;
                }

                foreach (var techNode in techGroup.Nodes)
                {
                    technologies.ServerAddNode(techNode);
                }
            }

            return $"{player} all tech groups of {tier} added {(withNodes ? "with nodes" : "without nodes")}).";
        }
    }
}