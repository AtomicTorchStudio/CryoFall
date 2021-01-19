namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectQuantumAcceleration : ProtoStatusEffect
    {
        public override string Description =>
            "Increases crafting speed by creating isolated quantum space around the user.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 60.0;

        public override StatusEffectKind Kind => StatusEffectKind.Buff;

        public override string Name => "Quantum acceleration";

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPercent(this, StatName.CraftingSpeed, 2500);
        }
    }
}