namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking;

    public class TechGroupCookingT2 : TechGroup
    {
        public override string Description => TechGroupsLocalization.CookingDescription;

        public override string Name => TechGroupsLocalization.CookingName;

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupCookingT1>(completion: 1);
        }
    }
}