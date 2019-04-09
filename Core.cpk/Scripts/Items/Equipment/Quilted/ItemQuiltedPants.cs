namespace AtomicTorch.CBND.CoreMod.Items.Equipment.Quilted
{
    public class ItemQuiltedPants : ProtoItemEquipmentLegs
    {
        public override string Description => GetProtoEntity<ItemQuiltedCoat>().Description;

        public override ushort DurabilityMax => 800;

        public override string Name => "Quilted pants";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.35,
                heat: 0.20,
                cold: 0.50,
                chemical: 0.15,
                electrical: 0.20,
                radiation: 0.15,
                psi: 0);
        }
    }
}