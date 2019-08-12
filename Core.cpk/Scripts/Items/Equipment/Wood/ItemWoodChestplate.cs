namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemWoodChestplate : ProtoItemEquipmentChest
    {
        public override string Description =>
            "Necessity is the mother of invention. Not the most comfy, but quite decent protection early on.";

        public override uint DurabilityMax => 500;

        public override string Name => "Wooden chestplate";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.40,
                kinetic: 0.40,
                heat: 0.15,
                cold: 0.10,
                chemical: 0.15,
                electrical: 0.20,
                radiation: 0.10,
                psi: 0);
        }
    }
}