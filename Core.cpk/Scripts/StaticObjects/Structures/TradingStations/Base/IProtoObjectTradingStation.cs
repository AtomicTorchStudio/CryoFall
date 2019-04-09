namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;

    public interface IProtoObjectTradingStation : IProtoObjectWithOwnersList
    {
        byte LotsCount { get; }

        byte StockItemsContainerSlotsCount { get; }
    }
}