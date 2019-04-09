namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry4
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry3;

    public class TechGroupIndustry4 : TechGroup
    {
        public override string Description =>
            "High-tech industrial technologies, enabling the most complex technological processes.";

        public override bool IsPrimary => true;

        public override string Name => "Industry 4";

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustry3>(completion: 1);
        }
    }
}