// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerSetEnergy : BaseConsoleCommand
    {
        public override string Description =>
            "Sets energy value to a player character. The energy value is automatically clamped into the available range.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setEnergy";

        public string Execute(float energy, [CurrentCharacterIfNull] ICharacter character)
        {
            var stats = PlayerCharacter.GetPublicState(character).CurrentStatsExtended;
            stats.SharedSetStaminaCurrent(energy);
            return $"{character} energy set to {stats.StaminaCurrent}";
        }
    }
}