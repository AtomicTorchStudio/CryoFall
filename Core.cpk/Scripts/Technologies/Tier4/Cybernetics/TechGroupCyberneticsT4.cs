namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Cybernetics
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Medicine;

    public class TechGroupCyberneticsT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.CyberneticsDescription;

        public override string Name => TechGroupsLocalization.CyberneticsName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupMedicineT3>(completion: 1);
            requirements.AddGroup<TechGroupIndustryT3>(completion: 0.2);
            requirements.AddGroup<TechGroupChemistryT3>(completion: 0.2);
        }
    }
}