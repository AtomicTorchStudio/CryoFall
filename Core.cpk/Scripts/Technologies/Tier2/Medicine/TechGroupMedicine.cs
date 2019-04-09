namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Medicine
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming;

    public class TechGroupMedicine : TechGroup
    {
        public override string Description => "Creation of various medical items.";

        public override string Name => "Medicine";

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupFarming>(completion: 0.4);
            requirements.AddGroup<TechGroupCooking>(completion: 0.4);
        }
    }
}