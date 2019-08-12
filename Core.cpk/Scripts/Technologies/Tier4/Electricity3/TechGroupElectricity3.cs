namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity3
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity2;

    public class TechGroupElectricity3 : TechGroup
    {
        public override string Description => "High-tech electrical infrastructure.";

        public override string Name => "Electricity 3";

        public override bool IsPrimary => true;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupElectricity2>(completion: 1);
        }
    }
}