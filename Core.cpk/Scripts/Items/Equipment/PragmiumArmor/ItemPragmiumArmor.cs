namespace AtomicTorch.CBND.CoreMod.Items.Equipment.PragmiumArmor
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemPragmiumArmor : ProtoItemEquipmentFullBody
    {
        public override string Description =>
            "Experimental armor incorporating a pragmium lattice embedded in thin metal plates as its structural foundation. Very light and durable.";

        public override uint DurabilityMax => 1200;

        public override bool IsHairVisible => false;

        public override bool IsHeadVisible => false;

        public override ObjectMaterial Material => ObjectMaterial.HardTissues;

        public override string Name => "Pragmium armor";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.70,
                kinetic: 0.70,
                heat: 0.70,
                cold: 0.70,
                chemical: 0.70,
                electrical: 0.70,
                radiation: 0.50,
                psi: 0.50);
        }
    }
}