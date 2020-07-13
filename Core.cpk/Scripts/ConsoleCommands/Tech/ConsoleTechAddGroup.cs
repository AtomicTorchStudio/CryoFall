// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Tech
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleTechAddGroup : BaseConsoleCommand
    {
        public override string Description => "Add a particular tech group to a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "tech.addGroup";

        public string Execute(TechGroup group, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var technologies = player.SharedGetTechnologies();
            technologies.ServerAddGroup(group);
            return $"{player} tech group {group.NameWithTierName} added.";
        }
    }
}