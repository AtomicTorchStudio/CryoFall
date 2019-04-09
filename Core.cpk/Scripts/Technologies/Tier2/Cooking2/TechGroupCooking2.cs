namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking2
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking;

    public class TechGroupCooking2 : TechGroup
    {
        public override string Description => "More advanced cooking recipes for a variety of new foods.";

        public override string Name => "Cooking 2";

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupCooking>(completion: 1);
        }
    }
}