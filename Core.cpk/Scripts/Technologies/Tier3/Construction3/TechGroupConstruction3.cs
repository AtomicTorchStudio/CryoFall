namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction3
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2;

    public class TechGroupConstruction3 : TechGroup
    {
        public override string Description => "Advanced constructions.";

        public override bool IsPrimary => true;

        public override string Name => "Construction 3";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupConstruction2>(completion: 1);
        }
    }
}