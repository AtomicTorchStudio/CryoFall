namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.ExoticWeapons
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense;

    public class TechGroupExoticWeapons : TechGroup
    {
        public override string Description => TechGroupsLocalization.ExoticWeaponsDescription;

        public override string Name => TechGroupsLocalization.ExoticWeaponsName;

        public override TechTier Tier => TechTier.Tier5;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupOffenseT4>(completion: 1.0);
            requirements.AddGroup<TechGroupChemistryT4>(completion: 1.0);
            requirements.AddGroup<TechGroupIndustryT4>(completion: 0.2);
        }
    }
}