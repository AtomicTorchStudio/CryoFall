namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Cooking3
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking2;

    public class TechGroupCooking3 : TechGroup
    {
        public override string Description => "Even more complex cooking recipes for true connoisseurs of fine dishes.";

        public override string Name => "Cooking 3";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupCooking2>(completion: 1);
        }
    }
}