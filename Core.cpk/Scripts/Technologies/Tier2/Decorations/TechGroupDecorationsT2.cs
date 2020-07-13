namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Decorations
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupDecorationsT2 : TechGroup
    {
        public override string Description => TechGroupsLocalization.DecorationsDescription;

        public override string Name => TechGroupsLocalization.DecorationsName;

        public override TechTier Tier => TechTier.Tier2;

        protected override double GroupUnlockPriceMultiplier => 0; // completely free to unlock, but has prerequisites!

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustryT1>(completion: 1);
            requirements.AddGroup<TechGroupConstructionT1>(completion: 1);
        }
    }
}