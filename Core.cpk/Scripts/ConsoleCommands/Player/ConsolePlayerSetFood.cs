// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerSetFood : BaseConsoleCommand
    {
        public override string Description =>
            "Sets food value to a player character. The food value is automatically clamped into the available range.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setFood";

        public string Execute(float food, [CurrentCharacterIfNull] ICharacter character)
        {
            var stats = PlayerCharacter.GetPublicState(character).CurrentStatsExtended;
            stats.ServerSetFoodCurrent(food);
            return $"{character} food set to {stats.FoodCurrent}";
        }
    }
}