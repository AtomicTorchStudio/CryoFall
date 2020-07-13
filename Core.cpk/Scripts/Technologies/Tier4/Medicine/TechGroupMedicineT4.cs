namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Medicine
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Medicine;

    public class TechGroupMedicineT4 : TechGroup
    {
        public override string Description => TechGroupsLocalization.MedicineDescription;

        public override string Name => TechGroupsLocalization.MedicineName;

        public override TechTier Tier => TechTier.Tier4;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupMedicineT3>(completion: 1);
        }
    }
}