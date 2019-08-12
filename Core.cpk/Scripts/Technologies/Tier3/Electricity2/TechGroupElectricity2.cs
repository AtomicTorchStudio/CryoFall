namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity2
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Electricity;

    public class TechGroupElectricity2 : TechGroup
    {
        public override string Description => "Advanced electrical infrastructure.";

        public override string Name => "Electricity 2";

        public override bool IsPrimary => true;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupElectricity>(completion: 1);
        }
    }
}