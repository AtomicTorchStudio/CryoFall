namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Medicine2
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Medicine;

    public class TechGroupMedicine2 : TechGroup
    {
        public override string Description => "Allows for wider array of advanced medical items.";

        public override string Name => "Medicine 2";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupMedicine>(completion: 1);
        }
    }
}