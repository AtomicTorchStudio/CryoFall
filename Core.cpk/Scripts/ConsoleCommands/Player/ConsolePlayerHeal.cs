// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerHeal : BaseConsoleCommand
    {
        public override string Alias => "heal";

        public override string Description => "Restore 100% of the player character health, energy, etc.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.heal";

        public string Execute([CurrentCharacterIfNull] ICharacter player)
        {
            var stats = player.GetPublicState<PlayerCharacterPublicState>()
                              .CurrentStatsExtended;

            // override death doesn't work here as the dead character has no physics and simple healing will not help
            //stats.ServerSetHealthCurrent(stats.HealthMax, overrideDeath: true);

            stats.ServerSetCurrentValuesToMaxValues();
            player.ServerRemoveAllStatusEffects(removeOnlyDebuffs: true);

            CharacterDamageTrackingSystem.ServerClearStats(player);

            return string.Format(
                "{0} healed to {1} HP (all other stats are also restored, all debuff status effects were removed).",
                player,
                stats.HealthCurrent);
        }
    }
}