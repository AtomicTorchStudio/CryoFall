namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemBoneArmor : ProtoItemEquipmentArmor
    {
        public override string Description =>
            "Bone armor provides decent protection, given its price, and can be quite useful early on.";

        public override uint DurabilityMax => 500;

        public override ObjectMaterial Material => ObjectMaterial.HardTissues;

        public override string Name => "Bone armor";

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