namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using System.ComponentModel;

    public enum TradingStationLotState : byte
    {
        [Description("Available")]
        Available = 0,

        [Description("Out of stock")]
        OutOfStock = 1,

        [Description("No money")]
        NoMoney = 2,

        [Description("No space")]
        NoSpace = 3,

        [Description("Disabled")]
        Disabled = 4
    }
}