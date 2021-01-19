namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectDazed : ProtoStatusEffect
    {
        public const double MaxDuration = 4.0; // 4 seconds

        public override string Description => "You are dazed! Give it a few moments to get your senses back.";

        public override StatusEffectDisplayMode DisplayMode
            => StatusEffectDisplayMode.IconShowTimeRemains
               | StatusEffectDisplayMode.TooltipShowIntensityPercent
               | StatusEffectDisplayMode.TooltipShowTimeRemains;

        public override double IntensityAutoDecreasePerSecondValue => 1 / MaxDuration;

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Dazed";

        public override double ServerUpdateIntervalSeconds => 0.1;

        protected override void ClientDeinitialize(StatusEffectData data)
        {
            ClientComponentStatusEffectDazedManager.TargetIntensity = 0;
        }

        protected override void ClientUpdate(StatusEffectData data)
        {
            ClientComponentStatusEffectDazedManager.TargetIntensity = data.Intensity;
        }

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPerk(this, StatName.PerkCannotRun);

            // -50% movement speed
            effects.AddPercent(this, StatName.MoveSpeed, -50);
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            intensityToAdd *= data.Character.SharedGetFinalStatMultiplier(StatName.DazedIncreaseRateMultiplier);

            if (intensityToAdd <= 0)
            {
                return;
            }

            // otherwise add as normal
            base.ServerAddIntensity(data, intensityToAdd);
        }
    }
}