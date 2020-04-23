namespace AtomicTorch.CBND.CoreMod.Items.Implants
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemImplantNanofiberSkin : ProtoItemEquipmentImplant
    {
        public override string Description =>
            "This special nanofiber material can be incorporated into the lower epidermis layer, offering significant protective qualities.";

        public override string Name => "Nanofiber skin implant";

        protected override void PrepareEffects(Effects effects)
        {
            base.PrepareEffects(effects);

            // add 10% protection against everything except psi and radiation
            var protection = 0.1;
            effects.AddValue(this, StatName.DefenseImpact, protection)
                   .AddValue(this, StatName.DefenseKinetic,    protection)
                   .AddValue(this, StatName.DefenseHeat,       protection)
                   .AddValue(this, StatName.DefenseCold,       protection)
                   .AddValue(this, StatName.DefenseChemical,   protection)
                   .AddValue(this, StatName.DefenseElectrical, protection);
        }
    }
}