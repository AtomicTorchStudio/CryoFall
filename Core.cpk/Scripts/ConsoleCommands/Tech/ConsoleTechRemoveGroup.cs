// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Tech
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleTechRemoveGroup : BaseConsoleCommand
    {
        public override string Description => "Remove a tech group from a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "tech.removeGroup";

        public string Execute(TechGroup group, [CurrentCharacterIfNull] ICharacter player = null)
        {
            player.SharedGetTechnologies().ServerRemoveGroup(group);
            return $"{player} tech group {group.NameWithTierName} removed.";
        }
    }
}