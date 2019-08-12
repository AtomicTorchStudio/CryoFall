namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Canning
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Cooking3;

    public class TechGroupCanning : TechGroup
    {
        public override string Description => "Creation of canned food that stores for a very long time.";

        public override string Name => "Canning";

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupCooking3>(completion: 1);
            requirements.AddGroup<TechGroupIndustry2>(completion: 0.2);
            requirements.AddGroup<TechGroupChemistry>(completion: 0.2);
        }
    }
}