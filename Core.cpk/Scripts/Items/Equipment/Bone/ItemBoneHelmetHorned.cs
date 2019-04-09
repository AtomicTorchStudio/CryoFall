namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemBoneHelmetHorned : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemBoneJacket>().Description;

        public override ushort DurabilityMax => 500;

        public override bool IsHairVisible => false;

        public override string Name => "Horned bone helmet";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.3,
                heat: 0.2,
                cold: 0.2,
                chemical: 0.1,
                electrical: 0.15,
                radiation: 0.1,
                psi: 0);
        }
    }
}