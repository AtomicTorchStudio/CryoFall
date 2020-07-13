namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Jewelry
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;

    public class TechGroupJewelryT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.JewelryDescription;

        public override string Name => TechGroupsLocalization.JewelryName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupDecorationsT3>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT3>(completion: 1);
            requirements.AddGroup<TechGroupDefenseT3>(completion: 0.2);
        }
    }
}