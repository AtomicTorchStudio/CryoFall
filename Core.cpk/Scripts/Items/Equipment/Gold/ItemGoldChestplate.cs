namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemGoldChestplate : ProtoItemEquipmentChest
    {
        public override string Description => "Luxurious golden attire. Befitting of true rulers.";

        public override ushort DurabilityMax => 800;

        public override string Name => "Gold chestplate";

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
        }
    }
}