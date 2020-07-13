namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Medicine
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming;

    public class TechGroupMedicineT2 : TechGroup
    {
        public override string Description => TechGroupsLocalization.MedicineDescription;

        public override string Name => TechGroupsLocalization.MedicineName;

        public override TechTier Tier => TechTier.Tier2;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupFarmingT1>(completion: 0.4);
            requirements.AddGroup<TechGroupCookingT1>(completion: 0.4);
        }
    }
}