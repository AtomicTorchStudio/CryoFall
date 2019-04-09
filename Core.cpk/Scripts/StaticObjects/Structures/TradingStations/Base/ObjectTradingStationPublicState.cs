namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ObjectTradingStationPublicState : StaticObjectPublicState
    {
        [SyncToClient]
        public NetworkSyncList<TradingStationLot> Lots { get; set; }

        [SyncToClient]
        public TradingStationMode Mode { get; set; }
    }
}