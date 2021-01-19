namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectOvereating : ProtoStatusEffect
    {
        public override string Description =>
            "You've eaten more than you probably should have. Stuff in any more food and you will feel sick.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max time

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        public override string Name => "Overeating";

        public override double VisibilityIntensityThreshold => 0.5; // becomes visible when reaches 50%

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            if (data.Character.SharedHasPerk(StatName.PerkOvereatWithoutConsequences))
            {
                // the character can overeat without consequences
                return;
            }

            // add nausea when eating anything when overeating is already at 50%
            if (data.Intensity > 0.5)
            {
                data.Character.ServerAddStatusEffect<StatusEffectNausea>(0.5);
            }

            // increase the status effect intensity
            data.Intensity += intensityToAdd;
        }
    }
}