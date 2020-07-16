// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Party
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePartySetClanTag : BaseConsoleCommand
    {
        public override string Description =>
            "Sets the clan tag for the party of the specified player. The clan tag could be left empty - the clan tag will be removed for the party then.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "party.setClanTag";

        public string Execute(ICharacter player, string clanTag = null)
        {
            var party = PartySystem.ServerGetParty(player);
            if (party is null)
            {
                return $"Player \"{player.Name}\" has no party";
            }

            PartySystem.ServerSetClanTag(party, clanTag);
            return "Party clan tag set successfully";
        }
    }
}