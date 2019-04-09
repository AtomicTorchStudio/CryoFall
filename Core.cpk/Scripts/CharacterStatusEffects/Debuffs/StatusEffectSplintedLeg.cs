namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectSplintedLeg : ProtoStatusEffect
    {
        public override string Description =>
            "You have applied a splint to your fracture and it is slowly healing. You cannot really run with your leg in that condition, though. Give it some time.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // 10 minutes

        public override bool IsRemovedOnRespawn => false;

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Splinted leg";

        protected override void PrepareEffects(Effects effects)
        {
            // cannot run
            effects.AddPercent(this, StatName.MoveSpeedRunMultiplier, -100);
        }

        protected override void ServerSetup(StatusEffectData data)
        {
            data.Character.ServerRemoveStatusEffect<StatusEffectBrokenLeg>();
        }
    }
}