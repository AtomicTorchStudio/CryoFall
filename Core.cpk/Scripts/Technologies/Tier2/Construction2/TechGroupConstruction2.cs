namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;

    public class TechGroupConstruction2 : TechGroup
    {
        public override string Description => "More complex structures with wider useful applications.";

        public override bool IsPrimary => true;

        public override string Name => "Construction 2";

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupConstruction>(completion: 1);
        }
    }
}