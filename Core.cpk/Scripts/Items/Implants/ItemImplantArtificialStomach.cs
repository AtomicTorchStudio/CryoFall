namespace AtomicTorch.CBND.CoreMod.Items.Implants
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemImplantArtificialStomach : ProtoItemEquipmentImplant
    {
        public override string Description =>
            "Improves the stomach by incorporating stronger stomach lining, more potent acid and enzymes that break down or neutralize most harmful substances. With it you can eat nearly-spoiled food and drink dirty water without worries.";

        public override string Name => "Artificial stomach implant";

        protected override void PrepareEffects(Effects effects)
        {
            base.PrepareEffects(effects);

            // adds ability to eat spoiled food, drink dirty water, etc.
            effects.AddPerk(this, StatName.PerkEatSpoiledFood);

            // adds ability to overeat without consequences
            effects.AddPerk(this, StatName.PerkOvereatWithoutConsequences);
        }
    }
}