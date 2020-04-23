namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemBoneJacket : ProtoItemEquipmentChest
    {
        public override string Description =>
            "Bone armor provides decent protection, given its price, and could be quite useful early on.";

        public override uint DurabilityMax => 500;

        public override ObjectMaterial Material => ObjectMaterial.HardTissues;

        public override string Name => "Bone jacket";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.3,
                heat: 0.2,
                cold: 0.2,
                chemical: 0.1,
                electrical: 0.15,
                radiation: 0.1,
                psi: 0);
        }
    }
}