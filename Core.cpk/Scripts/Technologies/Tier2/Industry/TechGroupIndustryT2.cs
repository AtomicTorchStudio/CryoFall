namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupIndustryT2 : TechGroup
    {
        public override string Description => TechGroupsLocalization.IndustryDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.IndustryName;

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustryT1>(completion: 1);
        }
    }
}