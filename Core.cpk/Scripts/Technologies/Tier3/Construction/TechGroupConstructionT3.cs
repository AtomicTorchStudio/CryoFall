namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction;

    public class TechGroupConstructionT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ConstructionDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ConstructionName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupConstructionT2>(completion: 1);
        }
    }
}