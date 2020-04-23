namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Decorations
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupDecorations : TechGroup
    {
        public override string Description => "Why not make your surroundings look nicer?";

        public override string Name => "Decorations";

        public override TechTier Tier => TechTier.Tier2;

        protected override double GroupUnlockPriceMultiplier => 0; // completely free to unlock, but has prerequisites!

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustry>(completion: 1);
            requirements.AddGroup<TechGroupConstruction>(completion: 1);
        }
    }
}