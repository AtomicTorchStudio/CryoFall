namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Systems.CharacterStamina;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class PlayerCharacterCurrentStats : CharacterCurrentStats
    {
        [SyncToClient(
            receivers: SyncToClientReceivers.Owner,
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: 2)]
        public float FoodCurrent { get; private set; } = 100;

        [SyncToClient(
            receivers: SyncToClientReceivers.Owner,
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: 2)]
        public float FoodMax { get; private set; } = 100;

        [SyncToClient(
            receivers: SyncToClientReceivers.Owner,
            // Don't send changes - stamina system is completely simulated on Client-side.
            // Otherwise it will be impossible to make client movement to match Server-side when a player is out of stamina.
            isSendChanges: false)]
        public float StaminaCurrent { get; private set; } = 100;

        [SyncToClient(
            receivers: SyncToClientReceivers.Owner,
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: NetworkMaxStatUpdatesPerSecond)]
        public float StaminaMax { get; private set; } = 100;

        [SyncToClient(
            receivers: SyncToClientReceivers.Owner,
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: 2)]
        public float WaterCurrent { get; private set; } = 100;

        [SyncToClient(
            receivers: SyncToClientReceivers.Owner,
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: 2)]
        public float WaterMax { get; private set; } = 100;

        public override void ServerSetCurrentValuesToMaxValues()
        {
            base.ServerSetCurrentValuesToMaxValues();
            this.SharedSetStaminaCurrent(this.StaminaMax);
            this.ServerSetFoodCurrent(this.FoodMax);
            this.ServerSetWaterCurrent(this.WaterMax);
        }

        /// <summary>
        /// Set food - it will be clamped automatically.
        /// </summary>
        public void ServerSetFoodCurrent(float food)
        {
            this.SharedTryRefreshFinalCache();

            food = MathHelper.Clamp(food, min: 0, max: this.FoodMax);
            this.FoodCurrent = food;
        }

        public void ServerSetFoodMax(float maxFood)
        {
            this.FoodMax = maxFood;
            this.ServerSetFoodCurrent(this.FoodCurrent);
        }

        public void ServerSetStaminaMax(float staminaMax)
        {
            this.StaminaMax = staminaMax;
            this.SharedSetStaminaCurrent(this.StaminaCurrent);
        }

        /// <summary>
        /// Set water - it will be clamped automatically.
        /// </summary>
        public void ServerSetWaterCurrent(float water)
        {
            this.SharedTryRefreshFinalCache();

            water = MathHelper.Clamp(water, min: 0, max: this.WaterMax);
            this.WaterCurrent = water;
        }

        public void ServerSetWaterMax(float maxWater)
        {
            this.WaterMax = maxWater;
            this.ServerSetWaterCurrent(this.WaterCurrent);
        }

        /// <summary>
        /// Set stamina - it will be clamped automatically.
        /// </summary>
        public void SharedSetStaminaCurrent(float stamina, bool notifyClient = true)
        {
            this.SharedTryRefreshFinalCache();

            stamina = MathHelper.Clamp(stamina, min: 0, max: this.StaminaMax);
            var deltaStamina = stamina - this.StaminaCurrent;
            if (deltaStamina == 0)
            {
                return;
            }

            if (notifyClient
                && Api.IsServer)
            {
                var character = (ICharacter)this.GameObject;
                if (!character.IsNpc)
                {
                    CharacterStaminaSystem.ServerNotifyClientStaminaChange(character, deltaStamina);
                }
            }

            this.StaminaCurrent = stamina;
        }
    }
}