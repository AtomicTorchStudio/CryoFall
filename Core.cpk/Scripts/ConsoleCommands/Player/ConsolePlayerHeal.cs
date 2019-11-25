// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Vehicles;
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

            string vehicleHealedMessage = null;
            var vehicle = PlayerCharacter.GetPublicState(player).CurrentVehicle;
            if (!(vehicle is null))
            {
                var protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
                var vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
                vehiclePublicState.StructurePointsCurrent = protoVehicle.SharedGetStructurePointsMax(vehicle);
                vehicleHealedMessage = $"Vehicle healed to {vehiclePublicState.StructurePointsCurrent} HP";
            }

            return string.Format(
                "{0} healed to {1} HP (all other stats are also restored, all debuff status effects were removed){2}.",
                player,
                stats.HealthCurrent,
                vehicleHealedMessage != null
                    ? Environment.NewLine + vehicleHealedMessage
                    : string.Empty);
        }
    }
}