namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemFurArmor : ProtoItemEquipmentArmor
    {
        public override string Description =>
            "Decent early armor; especially suitable to protect against cold. Not very expensive, and it's comfy!";

        public override uint DurabilityMax => 800;

        public override ObjectMaterial Material => ObjectMaterial.SoftTissues;

        public override string Name => "Fur coat";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.40,
                explosion: 0.45,
                heat: 0.15,
                cold: 0.60,
                chemical: 0.15,
                radiation: 0.20,
                psi: 0.0);
        }
    }
}