namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemInsectMeatRaw : ProtoItemFood
    {
        public override string Description =>
            "Disgusting insect meat. Maybe cooking it will make it slightly less disgusting.";

        public override float FoodRestore => 3;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override bool IsAvailableInCompletionist => false;

        public override string Name => "Raw insect meat";

        public override ushort OrganicValue => 5;

        public override float StaminaRestore => -100;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                // adds food poisoning unless you have artificial stomach
                .WillAddEffect<StatusEffectNausea>(intensity: 0.75,
                                                   condition: context => !context.Character.SharedHasPerk(
                                                                             StatName.PerkEatSpoiledFood));
        }
    }
}