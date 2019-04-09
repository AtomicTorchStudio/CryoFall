namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ObjectTradingStationPrivateState : StructurePrivateState, IObjectWithOwnersPrivateState
    {
        [TempOnly]
        public ushort? LastStockItemsContainerHash { get; set; }

        [SyncToClient]
        public NetworkSyncList<string> Owners { get; set; }

        [SyncToClient]
        public TradingStationStatistics Statistics { get; private set; }

        [SyncToClient]
        public IItemsContainer StockItemsContainer { get; set; }
    }
}