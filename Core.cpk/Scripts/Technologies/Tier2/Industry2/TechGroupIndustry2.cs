namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupIndustry2 : TechGroup
    {
        public override string Description =>
            "Improved industrial technologies allow for more industrial processes and applications.";

        public override bool IsPrimary => true;

        public override string Name => "Industry 2";

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustry>(completion: 1);
        }
    }
}