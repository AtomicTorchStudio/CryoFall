namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemLeatherHelmetTricorne : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemLeatherArmor>().Description;

        public override uint DurabilityMax => 800;

        public override bool IsHairVisible => false;

        public override string Name => "Pirate hat";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.50,
                kinetic: 0.40,
                explosion: 0.40,
                heat: 0.35,
                cold: 0.30,
                chemical: 0.20,
                radiation: 0.20,
                psi: 0.0);
        }
    }
}