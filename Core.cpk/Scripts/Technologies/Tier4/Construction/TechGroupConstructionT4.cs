namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction;

    public class TechGroupConstructionT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ConstructionDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ConstructionName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupConstructionT3>(completion: 1);
        }
    }
}