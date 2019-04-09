// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerSetWater : BaseConsoleCommand
    {
        public override string Description =>
            "Sets water value to a player character. The water value is automatically clamped into the available range.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setWater";

        public string Execute(float water, [CurrentCharacterIfNull] ICharacter character)
        {
            var stats = PlayerCharacter.GetPublicState(character).CurrentStatsExtended;
            stats.ServerSetWaterCurrent(water);
            return $"{character} water set to {stats.WaterCurrent}";
        }
    }
}