namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemClothHelmet : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemClothArmor>().Description;

        public override uint DurabilityMax => 300;

        public override bool IsHairVisible => false;

        public override string Name => "Cloth hat";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.25,
                kinetic: 0.20,
                explosion: 0.10,
                heat: 0.15,
                cold: 0.15,
                chemical: 0.10,
                radiation: 0.10,
                psi: 0.0);
        }
    }
}