namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemMeatRaw : ProtoItemFood
    {
        public override string Description =>
            "Raw meat. Can be prepared in a variety of ways. Eating it raw is probably not a very good idea...";

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override bool IsAvailableInCompletionist => false;

        public override string Name => "Raw meat";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => -100;

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