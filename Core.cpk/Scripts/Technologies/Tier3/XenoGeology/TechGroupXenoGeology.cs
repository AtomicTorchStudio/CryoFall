namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.XenoGeology
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2;

    public class TechGroupXenogeology : TechGroup
    {
        public override string Description => "Technologies necessary for exploitation of resources on alien worlds.";

        public override string Name => "Xenogeology";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustry2>(completion: 1);
            requirements.AddGroup<TechGroupConstruction2>(completion: 1);
            requirements.AddGroup<TechGroupChemistry>(completion: 1);
        }
    }
}