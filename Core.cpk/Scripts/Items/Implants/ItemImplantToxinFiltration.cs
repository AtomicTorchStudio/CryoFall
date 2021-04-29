namespace AtomicTorch.CBND.CoreMod.Items.Implants
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemImplantToxinFiltration : ProtoItemEquipmentImplant
    {
        public override string Description =>
            "Special organ that connects to a large artery and helps filter various toxins and poisons from the blood.";

        public override string Name => "Toxin filtration implant";

        protected override void PrepareEffects(Effects effects)
        {
            base.PrepareEffects(effects);

            effects.AddPercent(this, StatName.ToxinsIncreaseRateMultiplier, -85);
        }
    }
}