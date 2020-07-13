namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;

    public class TechGroupIndustryT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.IndustryDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.IndustryName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustryT3>(completion: 1);
        }
    }
}