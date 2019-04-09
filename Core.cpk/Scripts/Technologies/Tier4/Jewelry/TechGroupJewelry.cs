namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Jewelry
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense3;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry3;

    public class TechGroupJewelry : TechGroup
    {
        public override string Description => "Working with precious metals and jewelry.";

        public override string Name => "Jewelry";

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustry3>(completion: 1);
            requirements.AddGroup<TechGroupDefense3>(completion: 0.2);
        }
    }
}