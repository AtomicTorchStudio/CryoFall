namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemMilitaryArmor : ProtoItemEquipmentArmor
    {
        public override string Description =>
            "Advanced materials used in the manufacturing of this armor make it an ideal protection against small arms and other basic weapons.";

        public override uint DurabilityMax => 1000;

        public override ObjectMaterial Material => ObjectMaterial.HardTissues;

        public override string Name => "Military armor";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.50,
                kinetic: 0.60,
                explosion: 0.60,
                heat: 0.25,
                cold: 0.20,
                chemical: 0.30,
                radiation: 0.25,
                psi: 0.0);
        }
    }
}