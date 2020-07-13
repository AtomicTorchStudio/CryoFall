namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity;

    public class TechGroupElectricityT5 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ElectricityDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ElectricityName;

        public override TechTier Tier => TechTier.Tier5;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupElectricityT4>(completion: 1);
        }
    }
}