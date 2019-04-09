namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class TradingStationLot : BaseNetObject
    {
        public const int MaxLotQuantity = 9999;

        public const ushort MaxPrice = 999;

        [SyncToClient]
        public uint CountAvailable { get; set; }

        [SyncToClient]
        public IItem ItemOnSale { get; set; }

        [SyncToClient]
        public ushort LotQuantity { get; private set; } = 1;

        [SyncToClient]
        public ushort PriceCoinPenny { get; private set; }

        [SyncToClient]
        public ushort PriceCoinShiny { get; private set; }

        [SyncToClient]
        public IProtoItem ProtoItem { get; set; }

        [SyncToClient]
        public TradingStationLotState State { get; set; }

        public void SetLotQuantity(ushort lotQuantity)
        {
            this.LotQuantity = MathHelper.Clamp(lotQuantity, 1, MaxLotQuantity);
        }

        public void SetPrices(ushort priceCoinPenny, ushort priceCoinShiny)
        {
            this.PriceCoinPenny = Math.Min(priceCoinPenny, MaxPrice);
            this.PriceCoinShiny = Math.Min(priceCoinShiny, MaxPrice);
        }

        public override string ToString()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, Trading station: {8}",
                                 nameof(this.ProtoItem),
                                 this.ProtoItem,
                                 nameof(this.CountAvailable),
                                 this.CountAvailable,
                                 nameof(this.PriceCoinPenny),
                                 this.PriceCoinPenny,
                                 nameof(this.PriceCoinShiny),
                                 this.PriceCoinShiny,
                                 this.GameObject);
        }
    }
}