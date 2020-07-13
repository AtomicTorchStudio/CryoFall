namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Cooking
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Cooking;

    public class TechGroupCookingT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.CookingDescription;

        public override string Name => TechGroupsLocalization.CookingName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupCookingT3>(completion: 1);
        }
    }
}