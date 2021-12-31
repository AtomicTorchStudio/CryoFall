namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemWoodHelmet : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemWoodArmor>().Description;

        public override uint DurabilityMax => 500;

        public override bool IsHairVisible => false;

        public override string Name => "Wooden helmet";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.40,
                kinetic: 0.40,
                explosion: 0.30,
                heat: 0.15,
                cold: 0.10,
                chemical: 0.15,
                radiation: 0.10,
                psi: 0);
        }
    }
}