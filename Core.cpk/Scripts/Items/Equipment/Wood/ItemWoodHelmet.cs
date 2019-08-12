namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemWoodHelmet : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemWoodChestplate>().Description;

        public override uint DurabilityMax => 500;

        public override bool IsHairVisible => false;

        public override string Name => "Wooden helmet";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.40,
                kinetic: 0.40,
                heat: 0.15,
                cold: 0.10,
                chemical: 0.15,
                electrical: 0.20,
                radiation: 0.10,
                psi: 0);
        }
    }
}