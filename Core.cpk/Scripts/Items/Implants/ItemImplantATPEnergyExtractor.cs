namespace AtomicTorch.CBND.CoreMod.Items.Implants
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemImplantATPEnergyExtractor : ProtoItemEquipmentImplant
    {
        public override string Description =>
            "Uses natural metabolism to extract energy directly from ATP in the cells and convert it to electrical energy at the cost of increased hunger. Requires an equipped powerbank device to have any effect.";

        public override string Name => "ATP energy extractor implant";

        protected override void PrepareEffects(Effects effects)
        {
            base.PrepareEffects(effects);

            // will produce some energy (EU) every minute
            effects.AddValue(this, StatName.EnergyChargeRegenerationPerMinute, 60);

            // will consume 100% more food
            effects.AddPercent(this, StatName.FoodConsumptionSpeedMultiplier, 100);
        }
    }
}