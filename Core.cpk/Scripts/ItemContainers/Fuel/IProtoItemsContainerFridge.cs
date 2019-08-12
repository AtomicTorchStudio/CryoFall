namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    /// <summary>
    /// Items container which reduces food spoilage rate.
    /// </summary>
    public interface IProtoItemsContainerFridge : IProtoItemsContainer
    {
        /// <summary>
        /// Gets the current food spoilage rate for the container instance of this type.
        /// </summary>
        double SharedGetCurrentFoodFreshnessDecreaseCoefficient(IItemsContainer container);
    }
}