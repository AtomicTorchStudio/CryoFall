namespace AtomicTorch.CBND.CoreMod.Systems.FoodSpoilageSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class FoodSpoilageSystem : ProtoSystem<FoodSpoilageSystem>
    {
        private const uint FreshnessFractionsPerSecond = 1000;

        private static readonly IItemsServerService ServerItemsService = IsServer ? Server.Items : null;

        private static IProtoItem protoItemFoodRotten;

        public override string Name => "Food spoilage system";

        public static void ServerInitializeItem(IFoodPrivateState privateState, bool isFirstTimeInit)
        {
            var item = (IItem)privateState.GameObject;
            var protoItem = item.ProtoItem as IProtoItemFood
                            ?? throw new Exception(
                                $"The item {item} proto class doesn't implement {nameof(IProtoItem)}");

            privateState.FreshnessCurrent = isFirstTimeInit
                                                ? protoItem.FreshnessMaxValue
                                                : Math.Min(privateState.FreshnessCurrent, protoItem.FreshnessMaxValue);
        }

        public static void ServerUpdateFoodFreshness(IItem item, double deltaTime)
        {
            var protoItemFood = (IProtoItemFood)item.ProtoItem;
            if (protoItemFood.FreshnessMaxValue == 0)
            {
                // unlimited freshness
                return;
            }

            var privateState = item.GetPrivateState<IFoodPrivateState>();
            var freshness = (long)privateState.FreshnessCurrent;
            var freshnessDecrease = (uint)(deltaTime * FreshnessFractionsPerSecond);
            if (freshnessDecrease == 0)
            {
                // no freshness decrease
                return;
            }

            var container = item.Container;
            if (container == null)
            {
                // should be impossible - abandoned item
                return;
            }

            if (container.ProtoItemsContainer is IProtoItemsContainerFridge protoFridge)
            {
                var freshnessDecreaseCoefficient = protoFridge.GetFoodFreshnessDecreaseCoefficient(container);
                if (freshnessDecreaseCoefficient <= 0)
                {
                    // this fridge container stops spoilage!
                    return;
                }

                freshnessDecreaseCoefficient = Math.Min(freshnessDecreaseCoefficient, 1);
                if (freshnessDecreaseCoefficient < 1.0f)
                {
                    // calculate freshness decrease value 
                    freshnessDecrease = (uint)Math.Round(freshnessDecrease * freshnessDecreaseCoefficient,
                                                         MidpointRounding.AwayFromZero);

                    freshnessDecrease = Math.Max(freshnessDecrease, 1);
                }
            }

            freshness -= freshnessDecrease;

            if (freshness > 0)
            {
                privateState.FreshnessCurrent = (uint)freshness;
                return;
            }

            // food spoiled
            var containerSlotId = item.ContainerSlotId;
            var count = Math.Min(item.Count, protoItemFoodRotten.MaxItemsPerStack);

            // destroy food item
            ServerItemsService.DestroyItem(item);

            // spawn a rotten food item in place of the destroyed food item
            ServerItemsService.CreateItem(protoItemFoodRotten, container, count, containerSlotId);
        }

        public static uint SharedCalculateFreshnessMaxValue(IProtoItemFood protoItemFood)
        {
            var freshnessDuration = protoItemFood.FreshnessDuration;
            var freshnessMaxValue = FreshnessFractionsPerSecond * (ulong)freshnessDuration.TotalSeconds;
            if (freshnessMaxValue > uint.MaxValue)
            {
                throw new Exception(
                    $"Freshness duration exceeded max value of {TimeSpan.FromSeconds(uint.MaxValue / (double)FreshnessFractionsPerSecond)}."
                    + $" Provided value is {freshnessDuration}");
            }

            return (uint)freshnessMaxValue;
        }

        public static FoodFreshness SharedGetFreshnessEnum(IItem item)
        {
            var fraction = SharedGetFreshnessFraction(item);
            if (fraction > 0.5)
            {
                return FoodFreshness.Green;
            }

            if (fraction > 0.2)
            {
                return FoodFreshness.Yellow;
            }

            return FoodFreshness.Red;
        }

        public static double SharedGetFreshnessFraction(IItem item)
        {
            var protoItem = item.ProtoItem as IProtoItemFood
                            ?? throw new Exception(
                                item + " prototype doesn't implement " + typeof(IProtoItemFood));
            var freshnessMaxValue = protoItem.FreshnessMaxValue;
            if (freshnessMaxValue == 0)
            {
                // always fresh
                return 1.0;
            }

            var freshnessCurrent = item.GetPrivateState<IFoodPrivateState>().FreshnessCurrent;
            freshnessCurrent = MathHelper.Clamp(freshnessCurrent, 0, freshnessMaxValue);
            return freshnessCurrent / (double)freshnessMaxValue;
        }

        /// <summary>
        /// Gets the food positive effects coefficient depending on the food freshness.
        /// </summary>
        public static float SharedGetFreshnessPositiveEffectsCoef(FoodFreshness freshness)
        {
            switch (freshness)
            {
                case FoodFreshness.Green:
                    return 1.0f;

                case FoodFreshness.Yellow:
                    return 0.7f;

                case FoodFreshness.Red:
                    return 0.4f;

                default:
                    throw new ArgumentOutOfRangeException(nameof(freshness), freshness, null);
            }
        }

        protected override void PrepareSystem()
        {
            if (IsServer)
            {
                protoItemFoodRotten = Api.GetProtoEntity<ItemFoodRotten>();
            }
        }
    }
}