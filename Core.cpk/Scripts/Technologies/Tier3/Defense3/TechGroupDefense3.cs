namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense3
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense2;

    public class TechGroupDefense3 : TechGroup
    {
        public override string Description => "Advanced protection technologies.";

        public override string Name => "Defense 3";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupDefense2>(completion: 1);
        }
    }
}