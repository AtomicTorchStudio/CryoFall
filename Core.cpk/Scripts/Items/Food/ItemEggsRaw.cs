namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemEggsRaw : ProtoItemFood
    {
        public override string Description =>
            "Tasty no matter fried or boiled. Shouldn't be eaten raw, though, as that could lead to food poisoning.";

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsAvailableInCompletionist => false;

        public override string Name => "Animal eggs";

        public override ushort OrganicValue => 5;

        public override float WaterRestore => 5;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                // adds food poisoning unless you have artificial stomach
                .WillAddEffect<StatusEffectNausea>(intensity: 0.50,
                                                   condition: context => !context.Character.SharedHasPerk(
                                                                             StatName.PerkEatSpoiledFood));
        }
    }
}