// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Party
{
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePartyBrowse : BaseConsoleCommand
    {
        public override string Description => "Browse the party of the specified player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "party.browse";

        public string Execute(ICharacter player)
        {
            var party = PartySystem.ServerGetParty(player);
            if (party is null)
            {
                return $"Player \"{player.Name}\" has no party";
            }

            var memberNames = PartySystem.ServerGetPartyMembersReadOnly(party);
            var sb = new StringBuilder("Player \"")
                     .Append(player.Name)
                     .AppendLine("\" - party info:")
                     .AppendLine("Members list: ");

            foreach (var memberName in memberNames)
            {
                sb.Append(" * ").AppendLine(memberName);
            }

            return sb.ToString();
        }
    }
}