namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum TradingResult : byte
    {
        Success = 0,

        [Description("Cannot interact (too far or obstacles).")]
        ErrorCannotInteract = 3,

        [Description("Trading lot is not found.")]
        ErrorLotNotFound = 5,

        [Description("Trading lot is not active.")]
        ErrorLotNotActive = 10,

        [Description("Wrong trading mode.")]
        ErrorWrongMode = 20,

        [Description("Not enough items to sell.")]
        ErrorNotEnoughItemsOnPlayer = 30,

        [Description("Not enough items on station to buy.")]
        ErrorNotEnoughItemsOnStation = 35,

        [Description("Not enough money to buy this item.")]
        ErrorNotEnoughMoneyOnPlayer = 40,

        [Description("Not enough money on station to buy your item.")]
        ErrorNotEnoughMoneyOnStation = 45,

        [Description("Not enough space for purchased item.")]
        ErrorNotEnoughSpaceOnPlayerForPurchasedItem = 50,

        [Description("Not enough space on station to store the item you want to sell.")]
        ErrorNotEnoughSpaceOnStationForSoldItem = 55,

        [Description(CoreStrings.Item_CheckTooLowDurability)]
        ErrorTooLowDurability = 56,

        [Description(CoreStrings.Item_CheckTooLowFreshness)]
        ErrorTooLowFreshness = 57,

        ErrorItemPrototypeMismatch = 58
    }
}