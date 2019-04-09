namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.GameApi.Data.State;
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
    }
}