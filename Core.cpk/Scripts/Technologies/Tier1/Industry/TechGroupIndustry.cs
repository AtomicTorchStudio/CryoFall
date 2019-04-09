namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    public class TechGroupIndustry : TechGroup
    {
        public override string Description =>
            "All basic industrial technologies necessary to establish successful production and manufacturing.";

        public override bool IsPrimary => true;

        public override string Name => "Industry";

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}