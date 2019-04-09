namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemFurHatUshanka : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemFurCoat>().Description;

        public override ushort DurabilityMax => 800;

        public override bool IsHairVisible => false;

        public override string Name => "Ushanka hat";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.40,
                heat: 0.15,
                cold: 0.65,
                chemical: 0.15,
                electrical: 0.20,
                radiation: 0.20,
                psi: 0);
        }
    }
}