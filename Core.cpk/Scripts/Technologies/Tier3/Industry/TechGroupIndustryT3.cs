namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry;

    public class TechGroupIndustryT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.IndustryDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.IndustryName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustryT2>(completion: 1);
        }
    }
}