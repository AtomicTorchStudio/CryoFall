namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Cooking
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking;

    public class TechGroupCookingT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.CookingDescription;

        public override string Name => TechGroupsLocalization.CookingName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupCookingT2>(completion: 1);
        }
    }
}