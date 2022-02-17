namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.TradingStations;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class TradingStationLot : BaseNetObject
    {
        public const ushort MaxPrice = 9999;

        [SyncToClient]
        public uint CountAvailable { get; set; }

        [SyncToClient]
        public IItem ItemOnSale { get; set; }

        [SyncToClient]
        public ushort LotQuantity { get; private set; } = 1;

        /// <summary>
        /// Minimum durability/freshness percent. Used only when the station is buying.
        /// The station will check whether the item is above the threshold before purchasing it from player.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        [SyncToClient]
        public byte MinQualityPercent { get; set; }
            = TradingStationsSystem.DefaultMinQualityFractionWhenStationBuying;

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
            if (this.ProtoItem is null
                || !this.ProtoItem.IsStackable)
            {
                this.LotQuantity = 1;
                return;
            }

            this.LotQuantity = (ushort)MathHelper.Clamp((int)lotQuantity,
                                                        min: 1,
                                                        max: this.ProtoItem.MaxItemsPerStack);
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