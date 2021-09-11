namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RatePartyMembersMax
        : BaseRateByte<RatePartyMembersMax>
    {
        public override string Description =>
            @"How many party members are allowed in a party.";

        public override string Id => "PartyMembersMax";

        public override string Name => "Max members in a party";

        public override byte ValueDefault => 5;

        public override byte ValueMax => 20;

        public override byte ValueMin => 1;

        public override RateValueType ValueType => RateValueType.Number;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}