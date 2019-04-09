namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Farming3
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Farming2;

    public class TechGroupFarming3 : TechGroup
    {
        public override string Description =>
            "Advanced farming practices and even wider crop variety.";

        public override string Name => "Farming 3";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupFarming2>(completion: 1);
        }
    }
}