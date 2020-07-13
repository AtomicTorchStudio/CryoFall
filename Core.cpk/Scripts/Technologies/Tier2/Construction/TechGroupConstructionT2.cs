namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;

    public class TechGroupConstructionT2 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ConstructionDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ConstructionName;

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupConstructionT1>(completion: 1);
        }
    }
}