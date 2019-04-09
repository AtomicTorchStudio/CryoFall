namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.EnergyWeapons
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry3;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense3;

    public class TechGroupEnergyWeapons : TechGroup
    {
        public override string Description =>
            "Energy weapons are a new technological leap from conventional firearms.";

        public override string Name => "Energy weapons";

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupOffense3>(completion: 1);
            requirements.AddGroup<TechGroupIndustry3>(completion: 1);
        }
    }
}