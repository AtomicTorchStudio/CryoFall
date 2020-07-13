namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction
{
    public class TechGroupConstructionT1 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ConstructionDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ConstructionName;

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}