namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity;

    public class TechGroupElectricityT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ElectricityDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ElectricityName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupElectricityT3>(completion: 1);
        }
    }
}