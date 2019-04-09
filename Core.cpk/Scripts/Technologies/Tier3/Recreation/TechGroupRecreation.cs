namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Recreation
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Farming2;

    public class TechGroupRecreation : TechGroup
    {
        public override string Description => "Brewing alcoholic beverages and other things of that nature.";

        public override string Name => "Recreation";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupCooking2>(completion: 1.0);
            requirements.AddGroup<TechGroupFarming2>(completion: 1.0);
        }
    }
}