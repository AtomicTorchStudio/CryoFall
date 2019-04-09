namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction4
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction3;

    public class TechGroupConstruction4 : TechGroup
    {
        public override string Description => "High-tech constructions.";

        public override bool IsPrimary => true;

        public override string Name => "Construction 4";

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupConstruction3>(completion: 1);
        }
    }
}