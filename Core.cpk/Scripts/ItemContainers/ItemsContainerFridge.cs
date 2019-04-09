namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerFridge : ItemsContainerDefault, IProtoItemsContainerFridge
    {
        protected override bool IsValidateContainerInPrivateScope => false;

        public float GetFoodFreshnessDecreaseCoefficient(IItemsContainer container)
        {
            return 0.1f; // 10x food spoilage rate reduction
        }
    }
}