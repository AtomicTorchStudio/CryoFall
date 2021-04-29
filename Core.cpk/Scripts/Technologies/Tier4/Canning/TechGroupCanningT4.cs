namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Canning
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Cooking;

    public class TechGroupCanningT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.CanningDescription;

        public override string Name => TechGroupsLocalization.CanningName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupCookingT3>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT3>(completion: 0.2);
            requirements.AddGroup<TechGroupChemistryT3>(completion: 0.2);
        }
    }
}