namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupChemistry : TechGroup
    {
        public override string Description => "Working with various chemical compounds and reactions.";

        public override bool IsPrimary => true;

        public override string Name => "Chemistry";

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustry>(completion: 1);
            requirements.AddGroup<TechGroupConstruction>(completion: 0.8);
        }
    }
}