namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry2
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2;

    public class TechGroupChemistry2 : TechGroup
    {
        public override string Description => "Working with various complex chemical compounds and reactions.";

        public override bool IsPrimary => true;

        public override string Name => "Chemistry 2";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupChemistry>(completion: 1);
            requirements.AddGroup<TechGroupIndustry2>(completion: 0.6);
        }
    }
}