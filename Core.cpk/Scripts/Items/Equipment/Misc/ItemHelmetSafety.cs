namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemHelmetSafety : ProtoItemEquipmentHead
    {
        public override string Description =>
            "Safety helmets provide basic protection for the head from impacts, debris, electric shock and other hazards.";

        public override uint DurabilityMax => 600;

        public override bool IsHairVisible => false;

        public override string Name => "Safety helmet";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.50,
                kinetic: 0.35,
                explosion: 0.35,
                heat: 0.30,
                cold: 0.20,
                chemical: 0.30,
                radiation: 0.25,
                psi: 0.0);
        }
    }
}