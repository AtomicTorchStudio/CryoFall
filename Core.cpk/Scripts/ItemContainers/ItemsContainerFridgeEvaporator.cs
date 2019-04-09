namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerFridgeEvaporator : ItemsContainerDefault, IProtoItemsContainerFridge
    {
        protected override bool IsValidateContainerInPrivateScope => false;

        public float GetFoodFreshnessDecreaseCoefficient(IItemsContainer container)
        {
            return 0.2f; // 5x food spoilage rate reduction
        }
    }
}