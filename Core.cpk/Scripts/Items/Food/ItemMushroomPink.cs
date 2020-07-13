namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemMushroomPink : ProtoItemFood
    {
        public override string Description =>
            "Extremely toxic mushroom. So toxic it glows at night.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Lasting;

        public override bool IsAvailableInCompletionist => false;

        public override string Name => "Pink mushroom";

        public override ushort OrganicValue => 5;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            // shouldn't be eaten obviously :)
            effects
                // adds toxins
                .WillAddEffect<StatusEffectToxins>()
                // adds food poisoning unless you have artificial stomach
                .WillAddEffect<StatusEffectNausea>(condition: context => !context.Character.SharedHasPerk(
                                                                             StatName.PerkEatSpoiledFood));
        }
    }
}