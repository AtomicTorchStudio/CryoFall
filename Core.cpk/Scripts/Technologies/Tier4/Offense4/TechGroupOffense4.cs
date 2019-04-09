namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense4
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense3;

    public class TechGroupOffense4 : TechGroup
    {
        public override string Description =>
            "High-tech offensive technologies bringing conventional firearms to their maximum potential.";

        public override string Name => "Offense 4";

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupOffense3>(completion: 1);
        }
    }
}