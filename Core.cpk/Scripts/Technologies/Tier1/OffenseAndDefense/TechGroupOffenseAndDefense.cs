namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense
{
    public class TechGroupOffenseAndDefense : TechGroup
    {
        public override string Description => "Offers schematics for basic weapons, clothing and armor.";

        public override string Name => "Offense & Defense";

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}