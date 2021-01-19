namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum TradingStationMode : byte
    {
        StationSelling = 0,

        StationBuying = 1
    }
}