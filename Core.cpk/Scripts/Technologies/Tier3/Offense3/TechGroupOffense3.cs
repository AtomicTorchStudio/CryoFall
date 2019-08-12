namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense3
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense2;

    public class TechGroupOffense3 : TechGroup
    {
        public override string Description =>
            "Highly advanced offensive technologies, including fully automatic firearms and more diverse ammo types.";

        public override string Name => "Offense 3";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupOffense2>(completion: 1);
            requirements.AddGroup<TechGroupIndustry2>(completion: 0.2);
            requirements.AddGroup<TechGroupChemistry>(completion: 0.2);
        }
    }
}