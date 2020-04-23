namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs
{
    public class StatusEffectHealingFast : ProtoStatusEffect
    {
        // how much healing per second?
        private const double HealingPerSecond = 10.0;

        public override string Description =>
            "You have used powerful healing medicine and your body is quickly regaining lost health.";

        public override double IntensityAutoDecreasePerSecondValue
            => 0.1; // total of 10 seconds for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Buff;

        public override string Name => "Fast healing";

        public override double ServerUpdateIntervalSeconds => 0.1;

        protected override void ServerUpdate(StatusEffectData data)
        {
            // increase character health
            var stats = data.CharacterCurrentStats;
            var newHealth = stats.HealthCurrent + HealingPerSecond * data.DeltaTime;
            stats.ServerSetHealthCurrent((float)newHealth);
        }
    }
}