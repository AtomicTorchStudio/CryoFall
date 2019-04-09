namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Cybernetics
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry3;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Medicine2;

    public class TechGroupCybernetics : TechGroup
    {
        public override string Description => "Creation and application of cybernetic and biological implants.";

        public override string Name => "Cybernetics";

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupMedicine2>(completion: 1);
            requirements.AddGroup<TechGroupIndustry3>(completion: 0.2);
            requirements.AddGroup<TechGroupChemistry2>(completion: 0.2);
        }
    }
}