namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs
{
    public class StatusEffectHealingSlow : ProtoStatusEffect
    {
        // how much healing per second?
        private const double HealingPerSecond = 1.0;

        public override string Description =>
            "You have used some medicine and your body is slowly healing.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 100.0; // total of 100 seconds for max possible time, essentially +100hp max

        public override StatusEffectKind Kind => StatusEffectKind.Buff;

        public override string Name => "Healing";

        public override double ServerUpdateIntervalSeconds => 0.5;

        protected override void ServerUpdate(StatusEffectData data)
        {
            // increase character health
            var stats = data.CharacterCurrentStats;
            var newHealth = stats.HealthCurrent + HealingPerSecond * data.DeltaTime;
            stats.ServerSetHealthCurrent((float)newHealth);
        }
    }
}