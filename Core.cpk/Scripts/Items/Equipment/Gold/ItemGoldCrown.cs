namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemGoldCrown : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemGoldChestplate>().Description;

        public override ushort DurabilityMax => 800;

        public override bool IsHairVisible => false;

        public override string Name => "Gold crown";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.30,
                heat: 0.10,
                cold: 0.10,
                chemical: 0.25,
                electrical: 0.00,
                radiation: 0.25,
                psi: 0.0);

            // normal value override, we don't want it to be affected by armor multiplier later
            defense.Psi = 0.6 / defense.Multiplier;
        }
    }
}