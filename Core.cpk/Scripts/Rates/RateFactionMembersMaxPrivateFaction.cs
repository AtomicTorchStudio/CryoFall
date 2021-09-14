namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateFactionMembersMaxPrivateFaction
        : BaseRateUshort<RateFactionMembersMaxPrivateFaction>
    {
        [NotLocalizable]
        public override string Description =>
            @"How many faction members are allowed for a private faction
              (where players can join only by submitting an application or receiving an invite).";

        public override string Id => "Faction.MembersMax.PrivateFaction";

        public override string Name => "[Faction] Max members in private faction";

        public override ushort ValueDefault => 10;

        public override ushort ValueMax => 250;

        public override ushort ValueMaxReasonable => 25;

        public override ushort ValueMin => 1;

        public override RateValueType ValueType => RateValueType.Number;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}