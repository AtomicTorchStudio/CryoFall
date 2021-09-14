namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateFactionMembersMaxPublicFaction
        : BaseRateUshort<RateFactionMembersMaxPublicFaction>
    {
        [NotLocalizable]
        public override string Description =>
            @"How many faction members are allowed for a public faction
              (where anyone can join freely at any time).
              IMPORTANT: You can set this to 0 to disable public factions altogether.";

        public override string Id => "Faction.MembersMax.PublicFaction";

        public override string Name => "[Faction] Max members in public faction";

        public override ushort ValueDefault => 100;

        public override ushort ValueMax => 250;

        public override ushort ValueMin => 0;

        public override RateValueType ValueType => RateValueType.Number;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}