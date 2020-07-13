namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking
{
    public class TechGroupCookingT1 : TechGroup
    {
        public override string Description => TechGroupsLocalization.CookingDescription;

        public override string Name => TechGroupsLocalization.CookingName;

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}