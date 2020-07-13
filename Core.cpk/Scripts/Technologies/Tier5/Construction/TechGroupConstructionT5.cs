namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Construction
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction;

    public class TechGroupConstructionT5 : TechGroup
    {
        public override string Description => TechGroupsLocalization.ConstructionDescription;

        public override bool IsPrimary => true;

        public override string Name => TechGroupsLocalization.ConstructionName;

        public override TechTier Tier => TechTier.Tier5;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupConstructionT4>(completion: 1);
        }
    }
}