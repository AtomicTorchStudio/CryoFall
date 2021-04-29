namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using static Stats.StatName;

    /// <summary>
    /// This system will decrease food/water bars for online active players.
    /// If player is online but idle (AFK), food/water decrease will be suspended.
    /// </summary>
    public class CharacterHungerThirstSystem : ProtoSystem<CharacterHungerThirstSystem>
    {
        public const bool IsEnabledInEditor = false;

        // Active mode food decrease multiplier - change consumption speed when stamina is not full.
        // currently not used, same consumption speed
        private const double FoodDecreaseMultiplierWhenStaminaNotFull = 1.0;

        // Food/water update time interval.
        private const double TimeIntervalSeconds = 10.0;

        // Active mode water decrease multiplier - change consumption speed when stamina is not full.
        private const double WaterDecreaseMultiplierWhenStaminaNotFull = 2.0;

        // Passive mode food decrease (set in static constructor).
        private static readonly double FoodDecreasePerSecond;

        // Passive mode water decrease (set in static constructor).
        private static readonly double WaterDecreasePerSecond;

        static CharacterHungerThirstSystem()
        {
            if (IsClient)
            {
                // only server will update food and water values
                return;
            }

            var foodDecreaseSpeedMultiplier = ServerRates.Get(
                "PlayerFoodDecreaseSpeedMultiplier",
                defaultValue: 1.0,
                @"Food consumption speed multiplier for hunger mechanic. 
                  By default the game will consume 100 food points in 1.2 hours,
                  you can make it faster or slower, or disable altogether.");

            var waterDecreaseSpeedMultiplier = ServerRates.Get(
                "PlayerWaterDecreaseSpeedMultiplier",
                defaultValue: 1.0,
                @"Water consumption speed multiplier for thirst mechanic.
                  By default the game will consume 100 water points in 1 hour,
                  you can make it faster or slower, or disable altogether.
                  Please note the game will consume water twice as fast if player's stamina is regenerating.");

            if (foodDecreaseSpeedMultiplier < 0)
            {
                foodDecreaseSpeedMultiplier = 0;
            }

            if (waterDecreaseSpeedMultiplier < 0)
            {
                waterDecreaseSpeedMultiplier = 0;
            }

            FoodDecreasePerSecond = 100.0 / (60.0 * 60.0 * 1.2);  // consume 100 food points in 1.2 hour(s).
            FoodDecreasePerSecond *= foodDecreaseSpeedMultiplier; // apply the multiplier

            WaterDecreasePerSecond = 100.0 / (60.0 * 60.0 * 1.0);   // consume 100 water points in 1.0 hour(s)
            WaterDecreasePerSecond *= waterDecreaseSpeedMultiplier; // apply the multiplier
        }

        public override string Name => "Hunger and thirst system";

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                // only server will update food and water values
                return;
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            if (Api.IsEditor
                && !IsEnabledInEditor)
            {
                return;
            }

            // configure time interval trigger
            TriggerTimeInterval.ServerConfigureAndRegister(
                interval: TimeSpan.FromSeconds(TimeIntervalSeconds),
                callback: ServerTimerTickCallback,
                name: "System." + this.ShortId);
        }

        private static void ServerTimerTickCallback()
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

                if (CharacterIdleSystem.CharacterIdleSystem.ServerIsIdlePlayer(character))
                {
                    // idle character
                    continue;
                }

                var stats = publicState.CurrentStatsExtended;

                // please note: multipliers are summed before use (with decreasing each multiplier on 1.0 as it's the initial value)
                var waterDecreaseMultiplier = 1.0;
                var foodDecreaseMultiplier = 1.0;

                if (stats.StaminaCurrent < stats.StaminaMax)
                {
                    // consume more water and food when stamina is not full (goes to stamina regeneration)
                    waterDecreaseMultiplier += WaterDecreaseMultiplierWhenStaminaNotFull - 1.0;
                    foodDecreaseMultiplier += FoodDecreaseMultiplierWhenStaminaNotFull - 1.0;
                }

                // apply stat effects
                waterDecreaseMultiplier *= character.SharedGetFinalStatMultiplier(ThirstRate);
                foodDecreaseMultiplier *= character.SharedGetFinalStatMultiplier(HungerRate);

                // calculate decrease values
                var waterDecrease = WaterDecreasePerSecond * TimeIntervalSeconds * waterDecreaseMultiplier;
                var foodDecrease = FoodDecreasePerSecond * TimeIntervalSeconds * foodDecreaseMultiplier;

                stats.ServerSetWaterCurrent((float)(stats.WaterCurrent - waterDecrease));
                stats.ServerSetFoodCurrent((float)(stats.FoodCurrent - foodDecrease));
            }
        }
    }
}