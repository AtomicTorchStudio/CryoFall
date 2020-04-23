// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Party;

    public class ConsoleAdminResetClanTag : BaseConsoleCommand
    {
        public override string Description => "Finds a clan tag and removes it. It can be selected by another team.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.resetClanTag";

        public string Execute(string clanTag)
        {
            var party = PartySystem.Instance.ServerFindPartyByClanTag(clanTag);
            if (party == null)
            {
                return $"The clan tag {clanTag} is not used";
            }

            PartySystem.Instance.ServerSetClanTag(party, null);
            return $"The clan tag {clanTag} is released now";
        }
    }
}