namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemBoneHelmet : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemBoneArmor>().Description;

        public override uint DurabilityMax => 500;

        public override bool IsHairVisible => false;

        public override string Name => "Bone helmet";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.30,
                explosion: 0.25,
                heat: 0.20,
                cold: 0.20,
                chemical: 0.10,
                radiation: 0.10,
                psi: 0.0);
        }
    }
}