namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction
{
    public class TechGroupConstruction : TechGroup
    {
        public override string Description =>
            "Contains schematics for all basic building types and allows one to establish a simple base.";

        public override bool IsPrimary => true;

        public override string Name => "Construction";

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}