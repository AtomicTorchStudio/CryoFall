namespace AtomicTorch.CBND.CoreMod.Items.Equipment.Assault
{
    public class ItemAssaultJacket : ProtoItemEquipmentChest
    {
        public override string Description =>
            "This special assault armor provides amazing protection against enemy fire without compromising in other areas.";

        public override ushort DurabilityMax => 1200;

        public override string Name => "Assault armor jacket";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.65,
                kinetic: 0.75,
                heat: 0.3,
                cold: 0.3,
                chemical: 0.4,
                electrical: 0.4,
                radiation: 0.3,
                psi: 0.0);
        }
    }
}