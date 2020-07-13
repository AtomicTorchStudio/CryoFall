namespace AtomicTorch.CBND.CoreMod.Items.Implants
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemImplantReinforcedBones : ProtoItemEquipmentImplant
    {
        public override string Description =>
            "Artificial bone and cranium reinforcements. Prevents user from being dazed or having any bone fractures.";

        public override string Name => "Endoskeletal reinforcement implant";

        protected override void PrepareEffects(Effects effects)
        {
            base.PrepareEffects(effects);

            // Dazed protection (-100%, dazed effect cannot be added).
            effects.AddPercent(this, StatName.DazedIncreaseRateMultiplier, -100);

            // Broken bones protection.
            effects.AddValue(this, StatName.ReinforcedBones, 1);
        }
    }
}