namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Farming2
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupFarming2 : TechGroup
    {
        public override string Description =>
            "Complex farming techniques to work with a wider array of crops and better fertilizers.";

        public override string Name => "Farming 2";

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupFarming>(completion: 1);
            requirements.AddGroup<TechGroupIndustry>(completion: 0.6);
        }
    }
}