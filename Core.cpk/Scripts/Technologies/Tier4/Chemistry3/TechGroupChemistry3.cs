namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Chemistry3
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry2;

    public class TechGroupChemistry3 : TechGroup
    {
        public override string Description => "Advanced chemical processes and complex reactions.";

        public override bool IsPrimary => true;

        public override string Name => "Chemistry 3";

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupChemistry2>(completion: 1);
        }
    }
}