namespace AtomicTorch.CBND.CoreMod.Items.Equipment.Quilted
{
    public class ItemQuiltedHelmetPadded : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemQuiltedArmor>().Description;

        public override uint DurabilityMax => 800;

        public override bool IsHairVisible => false;

        public override string Name => "Quilted padded helmet";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.35,
                explosion: 0.40,
                heat: 0.25,
                cold: 0.50,
                chemical: 0.15,
                radiation: 0.15,
                psi: 0.0);
        }
    }
}