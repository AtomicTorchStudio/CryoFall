namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking
{
    public class TechGroupCooking : TechGroup
    {
        public override string Description => "Contains a number of recipes to enable production of better food items.";

        public override string Name => "Cooking";

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}