namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemMushroomRust : ProtoItemFood
    {
        public override string Description =>
            "These mushrooms are called rustshrooms for their deep color. They are quite tasty when cooked, but poisonous when eaten raw.";

        public override float FoodRestore => 3;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Lasting;

        public override bool IsAvailableInCompletionist => false;

        public override string Name => "Rustshroom";

        public override ushort OrganicValue => 5;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            // shouldn't be eaten raw
            effects
                // adds toxins
                .WillAddEffect<StatusEffectToxins>(intensity: 0.20)
                // adds food poisoning unless you have artificial stomach
                .WillAddEffect<StatusEffectNausea>(intensity: 0.50,
                                                   condition: context => !context.Character.SharedHasPerk(
                                                                             StatName.PerkEatSpoiledFood));
        }
    }
}