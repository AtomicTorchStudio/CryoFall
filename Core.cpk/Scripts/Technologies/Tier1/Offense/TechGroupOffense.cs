namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Offense
{
    public class TechGroupOffense : TechGroup
    {
        public override string Description => "Schematics for primitive weapons.";

        public override string Name => "Offense";

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}