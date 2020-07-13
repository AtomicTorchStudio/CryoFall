namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Defense
{
    public class TechGroupDefenseT1 : TechGroup
    {
        public override string Description => TechGroupsLocalization.DefenseDescription;

        public override string Name => TechGroupsLocalization.DefenseName;

        public override TechTier Tier => TechTier.Tier1;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            // no requirements
        }
    }
}