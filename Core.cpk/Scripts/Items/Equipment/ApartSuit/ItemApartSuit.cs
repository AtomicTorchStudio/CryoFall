namespace AtomicTorch.CBND.CoreMod.Items.Equipment.ApartSuit
{
    public class ItemApartSuit : ProtoItemEquipmentFullBody
    {
        public override string Description =>
            "Advanced environmental protection armor that offers the ideal combination of combat and environmental defense.";

        public override ushort DurabilityMax => 1200;

        public override bool IsHairVisible => false;

        // TODO: there is an issue with long beard, add sprite mask clipping and restore this to true
        public override bool IsHeadVisible => false;

        //  please don't translate A.P.A.R.T. abbreviation
        public override string Name => "A.P.A.R.T. suit";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.50,
                kinetic: 0.50,
                heat: 0.65,
                cold: 0.65,
                chemical: 0.75,
                electrical: 0.70,
                radiation: 0.75,
                psi: 0.60);
        }
    }
}