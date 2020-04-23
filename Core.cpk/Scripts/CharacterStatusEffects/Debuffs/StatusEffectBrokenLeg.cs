namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectBrokenLeg : ProtoStatusEffect
    {
        public override string Description =>
            "You have a broken leg, which affects your movement ability. Attempting to run with a broken leg will result in severe pain. Use a splint to heal the fracture.";

        // this status effect is not auto-decreased
        public override double IntensityAutoDecreasePerSecondValue => 0;

        public override bool IsRemovedOnRespawn => false;

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Broken leg";

        public override double ServerUpdateIntervalSeconds => 0.5;

        protected override void PrepareEffects(Effects effects)
        {
            // 20% move speed reduction
            // when we will implement the limping animation we will decrease the move speed even further
            effects.AddPercent(this, StatName.MoveSpeed, -20);
        }

        protected override void ServerSetup(StatusEffectData data)
        {
            data.Character.ServerRemoveStatusEffect<StatusEffectSplintedLeg>();
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            var appliedMoveMode = data.CharacterPublicState.AppliedInput.MoveModes;
            if ((appliedMoveMode & CharacterMoveModes.ModifierRun)
                == CharacterMoveModes.ModifierRun)
            {
                // character trying to run with broken leg
                data.Character.ServerAddStatusEffect<StatusEffectPain>(intensity: 0.5 * data.DeltaTime);
            }
        }
    }
}