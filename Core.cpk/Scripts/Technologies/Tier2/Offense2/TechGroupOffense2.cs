namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense2
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense;

    public class TechGroupOffense2 : TechGroup
    {
        public override string Description =>
            "More advanced offensive implements, including more powerful firearms and ammo types.";

        public override string Name => "Offense 2";

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupOffenseAndDefense>(completion: 1);
            requirements.AddGroup<TechGroupIndustry>(completion: 0.6);
        }
    }
}