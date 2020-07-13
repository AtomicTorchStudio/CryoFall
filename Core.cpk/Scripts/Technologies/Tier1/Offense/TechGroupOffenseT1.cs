namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Offense
{
    public class TechGroupOffenseT1 : TechGroup
    {
        public override string Description => TechGroupsLocalization.OffenseDescription;

        public override string Name => TechGroupsLocalization.OffenseName;

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}