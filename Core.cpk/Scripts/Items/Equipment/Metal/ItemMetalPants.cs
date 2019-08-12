namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemMetalPants : ProtoItemEquipmentLegs
    {
        public override string Description => GetProtoEntity<ItemMetalChestplate>().Description;

        public override uint DurabilityMax => 1000;

        public override string Name => "Metal leg armor";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.60,
                kinetic: 0.45,
                heat: 0.20,
                cold: 0.10,
                chemical: 0.15,
                electrical: 0.00,
                radiation: 0.10,
                psi: 0);

            // normal value override, we don't want it to be affected by armor multiplier later
            defense.Psi = 0.15 / defense.Multiplier;
        }
    }
}