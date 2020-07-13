namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    public class TechGroupIndustryT1 : TechGroup
    {
        public override string Description => TechGroupsLocalization.IndustryDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.IndustryName;

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}