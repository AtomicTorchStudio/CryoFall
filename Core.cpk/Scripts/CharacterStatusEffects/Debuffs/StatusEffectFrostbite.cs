namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectFrostbite : ProtoStatusEffect
    {
        public const double DamagePerSecondByIntensity = 1;

        public const double EffectAccumulationDuration = 60; // 100% intensity will accumulate in 1 minute

        public const double EffectCooldownDuration = 30; // 100% intensity will drop to 0% in 30 seconds

        private StatusEffectCold serverStatusEffectCold;

        public override string Description =>
            "You are freezing to death. Quickly put on warm clothing or move to a warm area.";

        // this status effect is not auto-decreased
        public override double IntensityAutoDecreasePerSecondValue => 0;

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Frostbite";

        public override double ServerUpdateIntervalSeconds => 0.5;

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddValue(this, StatName.VanityContinuousDamage, 1);
            effects.AddPerk(this, StatName.PerkCannotRun);
        }

        protected override void PrepareProtoStatusEffect()
        {
            base.PrepareProtoStatusEffect();
            if (IsServer)
            {
                this.serverStatusEffectCold = GetProtoEntity<StatusEffectCold>();
            }
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            if (data.Character.SharedGetStatusEffectIntensity(this.serverStatusEffectCold)
                < 1.0)
            {
                data.Intensity = Math.Max(data.Intensity - data.DeltaTime * (1.0 / EffectCooldownDuration),
                                          0);
            }

            var damage = DamagePerSecondByIntensity
                         * Math.Pow(data.Intensity, 1.5)
                         * data.DeltaTime;

            data.CharacterCurrentStats.ServerReduceHealth(damage, data.StatusEffect);
        }
    }
}