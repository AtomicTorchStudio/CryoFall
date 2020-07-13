namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Electricity;

    public class TechGroupElectricityT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ElectricityDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ElectricityName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupElectricityT2>(completion: 1);
        }
    }
}