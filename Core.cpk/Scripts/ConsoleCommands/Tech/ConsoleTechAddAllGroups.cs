// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Tech
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleTechAddAllGroups : BaseConsoleCommand
    {
        public override string Description => "Add all tech groups to a player (without their nodes).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "tech.addAllGroups";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var technologies = player.SharedGetTechnologies();

            foreach (var techGroup in TechGroup.AvailableTechGroups)
            {
                technologies.ServerAddGroup(techGroup);
            }

            return $"{player} all tech groups added (without nodes).";
        }
    }
}