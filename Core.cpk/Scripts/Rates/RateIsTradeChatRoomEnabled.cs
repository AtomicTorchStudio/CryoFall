namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateIsTradeChatRoomEnabled
        : BaseRateBoolean<RateIsTradeChatRoomEnabled>
    {
        [NotLocalizable]
        public override string Description => @"Is ""Trade"" chat room available on this server?";

        public override string Id => "IsTradeChatRoomEnabled";

        public override string Name => "Trade chat room";

        public override bool ValueDefault => true;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}