namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class CharacterHungerThirstSystem : ProtoSystem<CharacterHungerThirstSystem>
    {
        // Passive mode food decrease - consume 100 food points in 1.2 hour(s).
        private const double FoodDecrease = 100.0 / (60.0 * 60.0 * 1.2);

        // Active mode food decrease multiplier - change consumption speed when energy is not full.
        private const double FoodDecreaseMultiplierWhenEnergyNotFull = 1.0;

        // Regeneration mode food decrease multiplier - change consumption speed when health is not full.
        private const double FoodDecreaseMultiplierWhenHealthNotFull = 1.0;

        // Starvation mode health decrease - consume 100 health points in 5 minutes.
        private const double HealthDecreaseStarvation = 100.0 / (60.0 * 5.0);

        // Thirst mode health decrease - consume 100 health points in 5 minutes.
        private const double HealthDecreaseThirst = 100.0 / (60.0 * 5.0);

        // Food/water update time interval.
        private const double TimeIntervalSeconds = 10.0;

        // Passive mode water decrease - consume 100 water points in 1.0 hour(s).
        private const double WaterDecrease = 100.0 / (60.0 * 60.0 * 1.0);

        // Active mode water decrease multiplier - change consumption speed when energy is not full.
        private const double WaterDecreaseMultiplierWhenEnergyNotFull = 2.0;

        public override string Name => "Food and thirst system";

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                // only server will update food and water values
                return;
            }

            // configure time interval trigger
            TriggerTimeInterval.ServerConfigureAndRegister(
                interval: TimeSpan.FromSeconds(TimeIntervalSeconds),
                callback: this.ServerTimerTickCallback,
                name: "System." + this.ShortId);
        }

        private void ServerTimerTickCallback()
        {
            // update water and food for all online player characters
            foreach (var character in Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true))
            {
                if (character.ProtoCharacter.GetType() != typeof(PlayerCharacter))
                {
                    // only characters of specific type (PlayerCharacter) are processed
                    continue;
                }

                var publicState = PlayerCharacter.GetPublicState(character);
                if (publicState.IsDead)
                {
                    // dead characters are not processed
                    continue;
                }

                var stats = publicState.CurrentStatsExtended;

                // please note: multipliers are summed before use (with decreasing each multiplier on 1.0 as it's initial value)
                var waterDecreaseMultiplier = 1.0;
                var foodDecreaseMultiplier = 1.0;

                if (stats.StaminaCurrent < stats.StaminaMax)
                {
                    // consume more water and food when energy is not full (goes to energy regeneration)
                    waterDecreaseMultiplier += WaterDecreaseMultiplierWhenEnergyNotFull - 1.0;
                    foodDecreaseMultiplier += FoodDecreaseMultiplierWhenEnergyNotFull - 1.0;
                }

                var currentHealth = stats.HealthCurrent;
                if (currentHealth < stats.HealthMax)
                {
                    // consume more food when health is not full (goes to health regeneration)
                    foodDecreaseMultiplier += FoodDecreaseMultiplierWhenHealthNotFull - 1.0;
                }

                // apply stat effects
                waterDecreaseMultiplier *=
                    character.SharedGetFinalStatMultiplier(StatName.WaterConsumptionSpeedMultiplier);
                foodDecreaseMultiplier *=
                    character.SharedGetFinalStatMultiplier(StatName.FoodConsumptionSpeedMultiplier);

                var water = stats.WaterCurrent - WaterDecrease * waterDecreaseMultiplier * TimeIntervalSeconds;
                var food = stats.FoodCurrent - FoodDecrease * foodDecreaseMultiplier * TimeIntervalSeconds;
                var health = (double)currentHealth;

                if (water <= 0) // thirst
                {
                    health -= HealthDecreaseThirst * TimeIntervalSeconds;
                }

                if (food <= 0) // starvation
                {
                    health -= HealthDecreaseStarvation * TimeIntervalSeconds;
                }

                stats.ServerSetWaterCurrent((float)water);
                stats.ServerSetFoodCurrent((float)food);

                var newHealth = (float)health;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (currentHealth != newHealth)
                {
                    stats.ServerSetHealthCurrent(newHealth);
                }
            }
        }
    }
}