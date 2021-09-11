namespace AtomicTorch.CBND.CoreMod.Rates
{
    public class RateFactionCreateCost
        : BaseRateUshort<RateFactionCreateCost>
    {
        public override string Description =>
            "Determines how many learning points are required in order to create a faction.";

        public override string Id => "Faction.CreateCostLearningPoints";

        public override string Name => "[Faction] Creation cost (LP)";

        public override ushort ValueDefault => 200;

        public override ushort ValueMax => 65001;

        public override ushort ValueMaxReasonable => 1000;

        public override ushort ValueMin => 1;

        public override ushort ValueStepChange => 100;

        public override RateValueType ValueType => RateValueType.Number;

        public override RateVisibility Visibility => RateVisibility.Advanced;
    }
}