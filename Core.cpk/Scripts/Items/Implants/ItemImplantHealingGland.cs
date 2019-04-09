namespace AtomicTorch.CBND.CoreMod.Items.Implants
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemImplantHealingGland : ProtoItemEquipmentImplant
    {
        public override string Description =>
            "Improves natural regeneration by producing additional stem cells and modulating chemical composition of the blood.";

        public override string Name => "Healing gland implant";

        protected override void PrepareEffects(Effects effects)
        {
            base.PrepareEffects(effects);

            // extra regeneration (5X the normal speed)
            effects.AddPercent(this, StatName.HealthRegenerationPerSecond, 400);
        }
    }
}