// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Faction
{
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleFactionBrowse : BaseConsoleCommand
    {
        public override string Description => "Browse the faction of the specified player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "faction.browse";

        public string Execute(ICharacter player)
        {
            var faction = FactionSystem.ServerGetFaction(player);
            if (faction is null)
            {
                return $"Player \"{player.Name}\" has no faction";
            }

            var memberEntries = FactionSystem.ServerGetFactionMembersReadOnly(faction);
            var sb = new StringBuilder("Player \"")
                     .Append(player.Name)
                     .AppendLine("\" - faction info:")
                     .Append("Clan tag: ")
                     .Append("[")
                     .Append(FactionSystem.SharedGetClanTag(faction))
                     .AppendLine("]")
                     .AppendLine("Members list (with roles): ");

            foreach (var entry in memberEntries)
            {
                sb.Append(" * ").AppendLine(entry.Name + " - " + entry.Role);
            }

            return sb.ToString();
        }
    }
}