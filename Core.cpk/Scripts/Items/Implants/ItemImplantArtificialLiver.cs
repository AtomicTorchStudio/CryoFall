namespace AtomicTorch.CBND.CoreMod.Items.Implants
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemImplantArtificialLiver : ProtoItemEquipmentImplant
    {
        public override string Description =>
            "Artificial bioengineered liver. Enables improved blood filtration, negating some medicine overuse effects.";

        public override string Name => "Artificial liver";

        protected override void PrepareEffects(Effects effects)
        {
            base.PrepareEffects(effects);

            effects.AddPercent(this, StatName.MedicineToxicityMultiplier, -50);
        }
    }
}