namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Decorations
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;

    public class TechGroupDecorationsT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.DecorationsDescription;

        public override string Name => TechGroupsLocalization.DecorationsName;

        public override TechTier Tier => TechTier.Tier4;

        protected override double GroupUnlockPriceMultiplier => 0; // completely free to unlock, but has prerequisites!

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupDecorationsT3>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT3>(completion: 1);
            requirements.AddGroup<TechGroupConstructionT3>(completion: 1);
        }
    }
}