namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Electricity
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupElectricity : TechGroup
    {
        public override string Description => "Basic electrical infrastructure.";

        public override bool IsPrimary => true;

        public override string Name => "Electricity";

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupConstruction>(completion: 1);
            requirements.AddGroup<TechGroupIndustry>(completion: 1);
        }
    }
}