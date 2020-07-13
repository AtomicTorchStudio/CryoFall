namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Medicine
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Medicine;

    public class TechGroupMedicineT3 : TechGroup
    {
        public override string Description => TechGroupsLocalization.MedicineDescription;

        public override string Name => TechGroupsLocalization.MedicineName;

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupMedicineT2>(completion: 1);
        }
    }
}