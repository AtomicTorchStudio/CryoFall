namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Medicine3
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Medicine2;

    public class TechGroupMedicine3 : TechGroup
    {
        public override string Description => "Development and application of the most advanced medical technology.";

        public override string Name => "Medicine 3";

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupMedicine2>(completion: 1);
        }
    }
}