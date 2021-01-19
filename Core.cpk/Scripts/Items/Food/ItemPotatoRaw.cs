namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemPotatoRaw : ProtoItemFood
    {
        public override string Description =>
            "Must be cooked before eating.";

        public override float FoodRestore => 4;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Lasting;

        public override bool IsAvailableInCompletionist => false;

        public override string Name => "Potato";

        public override ushort OrganicValue => 5;

        public override float WaterRestore => 0;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                // adds food poisoning unless you have artificial stomach
                .WillAddEffect<StatusEffectNausea>(intensity: 0.10,
                                                   condition: context => !context.Character.SharedHasPerk(
                                                                             StatName.PerkEatSpoiledFood));
        }
    }
}