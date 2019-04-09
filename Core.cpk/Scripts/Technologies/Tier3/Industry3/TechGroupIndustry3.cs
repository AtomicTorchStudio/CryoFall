namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry3
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2;

    public class TechGroupIndustry3 : TechGroup
    {
        public override string Description =>
            "Advanced industrial technologies allow for more complex industrial processes and applications.";

        public override bool IsPrimary => true;

        public override string Name => "Industry 3";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustry2>(completion: 1);
        }
    }
}