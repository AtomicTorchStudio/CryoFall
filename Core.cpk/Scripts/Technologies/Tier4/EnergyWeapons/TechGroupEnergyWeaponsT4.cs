namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.EnergyWeapons
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense;

    public class TechGroupEnergyWeaponsT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.EnergyWeaponsDescription;

        public override string Name => TechGroupsLocalization.EnergyWeaponsName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupOffenseT3>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT3>(completion: 1);
        }
    }
}