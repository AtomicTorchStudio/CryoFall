namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class TradingStationStatistics : BaseNetObject
    {
        [SyncToClient]
        public uint DaysInOperation { get; set; }

        [SyncToClient]
        public uint TotalSalesCoinPenny { get; set; }

        [SyncToClient]
        public uint TotalSalesCoinShiny { get; set; }

        [SyncToClient]
        public uint TotalTransactions { get; set; }

        [SyncToClient]
        public uint TransactionsToday { get; set; }

        [SyncToClient]
        public uint VisitsToday { get; set; }

        public void Reset()
        {
            this.DaysInOperation = 0;
            this.TotalSalesCoinPenny = 0;
            this.TotalSalesCoinShiny = 0;
            this.TotalTransactions = 0;
            this.TransactionsToday = 0;
            this.VisitsToday = 0;
        }
    }
}