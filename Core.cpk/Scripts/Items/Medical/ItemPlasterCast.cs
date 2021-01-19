namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;

    public class ItemPlasterCast : ItemSplint
    {
        public override string Description => "Plaster cast is used to set a broken bone and allow it to heal quickly.";

        public override string Name => "Plaster cast";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .Clear()
                // adds splinted leg status effect when player has a broken leg status effect
                .WillAddEffect<StatusEffectSplintedLeg>(
                    intensity: 0.33f, // fraction of the time of splint
                    condition: context => context.Character.SharedHasStatusEffect<StatusEffectBrokenLeg>(),
                    isHidden: true)
                // and remove broken leg
                .WillRemoveEffect<StatusEffectBrokenLeg>();
        }
    }
}