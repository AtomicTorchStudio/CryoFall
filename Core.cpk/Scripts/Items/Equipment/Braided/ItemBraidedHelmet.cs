namespace AtomicTorch.CBND.CoreMod.Items.Equipment.Braided
{
    public class ItemBraidedHelmet : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemBraidedChestplate>().Description;

        public override uint DurabilityMax => 500;

        public override bool IsHairVisible => false;

        public override string Name => "Braided helmet";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.40,
                kinetic: 0.30,
                heat: 0.20,
                cold: 0.20,
                chemical: 0.10,
                electrical: 0.15,
                radiation: 0.15,
                psi: 0);
        }
    }
}