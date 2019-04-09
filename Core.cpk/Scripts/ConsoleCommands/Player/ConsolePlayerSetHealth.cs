// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerSetHealth : BaseConsoleCommand
    {
        public override string Description =>
            "Sets health value to a player character. The health value is automatically clamped into the available range.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setHealth";

        public string Execute(float health, [CurrentCharacterIfNull] ICharacter character)
        {
            var stats = PlayerCharacter.GetPublicState(character).CurrentStatsExtended;
            stats.ServerSetHealthCurrent(health);
            return $"{character} health set to {stats.HealthCurrent}";
        }
    }
}