namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Defense;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class TechGroupDefenseT2 : TechGroup
    {
        public override string Description => TechGroupsLocalization.DefenseDescription;

        public override string Name => TechGroupsLocalization.DefenseName;

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupDefenseT1>(completion: 1.0);
            requirements.AddGroup<TechGroupIndustryT1>(completion: 0.6);
        }
    }
}