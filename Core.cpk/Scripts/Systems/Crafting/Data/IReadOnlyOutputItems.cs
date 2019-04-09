namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public interface IReadOnlyOutputItems
    {
        int Count { get; }

        IReadOnlyList<OutputItem> Items { get; }

        CreateItemResult TrySpawnToContainer(IItemsContainer toContainer);
    }
}