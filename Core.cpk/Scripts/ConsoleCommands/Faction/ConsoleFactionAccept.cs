// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Faction
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleFactionAccept : BaseConsoleCommand
    {
        public override string Description =>
            "Accepts player to the faction. The player must submit an application to the faction first.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "faction.accept";

        public string Execute(ICharacter player, string clanTag)
        {
            var faction = FactionSystem.ServerGetFactionByClanTag(clanTag);
            if (faction is null)
            {
                return "Faction not found: " + clanTag;
            }

            var result = FactionSystem.ServerForceAcceptApplication(player,
                                                                    faction);

            return "Faction application accept result: " + result.GetDescription();
        }
    }
}