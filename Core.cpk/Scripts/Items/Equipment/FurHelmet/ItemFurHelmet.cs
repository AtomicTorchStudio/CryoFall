namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemFurHelmet : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemFurArmor>().Description;

        public override uint DurabilityMax => 800;

        public override bool IsHairVisible => false;

        public override string Name => "Fur helmet";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.40,
                explosion: 0.45,
                heat: 0.15,
                cold: 0.60,
                chemical: 0.15,
                radiation: 0.20,
                psi: 0.0);
        }
    }
}