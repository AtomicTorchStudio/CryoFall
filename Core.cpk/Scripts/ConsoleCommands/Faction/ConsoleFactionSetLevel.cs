// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Faction
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ConsoleFactionSetLevel : BaseConsoleCommand
    {
        public override string Description => "Sets the faction level.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "faction.setLevel";

        public string Execute(string clanTag, byte level)
        {
            var faction = FactionSystem.ServerGetFactionByClanTag(clanTag);
            if (faction is null)
            {
                return "Faction not found: " + clanTag;
            }

            level = (byte)MathHelper.Clamp((int)level,
                                           min: 1,
                                           max: FactionConstants.MaxFactionLevel);

            Faction.GetPublicState(faction).Level = level;
            return "Faction level changed to: " + level;
        }
    }
}