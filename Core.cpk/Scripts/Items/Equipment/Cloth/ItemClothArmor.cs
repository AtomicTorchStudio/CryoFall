namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemClothArmor : ProtoItemEquipmentArmor
    {
        public override string Description =>
            "Doesn't really provide any actual protection. Could maybe stop a mosquito?";

        public override uint DurabilityMax => 300;

        public override ObjectMaterial Material => ObjectMaterial.SoftTissues;

        public override string Name => "Cloth shirt";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.25,
                kinetic: 0.20,
                explosion: 0.10,
                heat: 0.15,
                cold: 0.15,
                chemical: 0.10,
                radiation: 0.10,
                psi: 0.0);
        }
    }
}