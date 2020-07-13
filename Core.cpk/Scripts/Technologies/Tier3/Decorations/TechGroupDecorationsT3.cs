namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Decorations;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry;

    public class TechGroupDecorationsT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.DecorationsDescription;

        public override string Name => TechGroupsLocalization.DecorationsName;

        public override TechTier Tier => TechTier.Tier3;

        protected override double GroupUnlockPriceMultiplier => 0; // completely free to unlock, but has prerequisites!

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupDecorationsT2>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT2>(completion: 1);
            requirements.AddGroup<TechGroupConstructionT2>(completion: 1);
        }
    }
}