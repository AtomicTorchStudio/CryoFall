namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming
{
    public class TechGroupFarming : TechGroup
    {
        public override string Description => "Preparation of seeds for planting, basic fertilizer and more.";

        public override string Name => "Farming";

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}