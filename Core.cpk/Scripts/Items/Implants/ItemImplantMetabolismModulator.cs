namespace AtomicTorch.CBND.CoreMod.Items.Implants
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemImplantMetabolismModulator : ProtoItemEquipmentImplant
    {
        public override string Description =>
            "Improves metabolism efficiency, resulting in reduced expenditure of calories and liquids in the body.";

        public override string Name => "Metabolism modulator implant";

        protected override void PrepareEffects(Effects effects)
        {
            base.PrepareEffects(effects);

            // reduced food consumption
            effects.AddPercent(this, StatName.HungerRate, -40);

            // reduced water consumption
            effects.AddPercent(this, StatName.ThirstRate, -40);
        }
    }
}