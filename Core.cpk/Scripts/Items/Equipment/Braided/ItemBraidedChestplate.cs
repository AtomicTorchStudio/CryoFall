namespace AtomicTorch.CBND.CoreMod.Items.Equipment.Braided
{
    public class ItemBraidedChestplate : ProtoItemEquipmentChest
    {
        public override string Description =>
            "Not exactly the fanciest of outfits, but it could protect you in a pinch. Offers better environmental protection than closest alternatives.";

        public override ushort DurabilityMax => 500;

        public override string Name => "Braided chestplate";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.40,
                kinetic: 0.30,
                heat: 0.20,
                cold: 0.20,
                chemical: 0.10,
                electrical: 0.15,
                radiation: 0.15,
                psi: 0);
        }
    }
}