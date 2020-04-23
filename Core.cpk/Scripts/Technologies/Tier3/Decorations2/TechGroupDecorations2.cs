namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Decorations2
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Decorations;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2;

    public class TechGroupDecorations2 : TechGroup
    {
        public override string Description => "Customize your surroundings even more!";

        public override string Name => "Decorations 2";

        public override TechTier Tier => TechTier.Tier3;

        protected override double GroupUnlockPriceMultiplier => 0; // completely free to unlock, but has prerequisites!

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupDecorations>(completion: 1);
            requirements.AddGroup<TechGroupIndustry2>(completion: 1);
            requirements.AddGroup<TechGroupConstruction2>(completion: 1);
        }
    }
}