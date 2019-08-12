namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Defense
{
    public class TechGroupDefense : TechGroup
    {
        public override string Description => "Schematics for basic clothing and armor.";

        public override string Name => "Defense";

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}