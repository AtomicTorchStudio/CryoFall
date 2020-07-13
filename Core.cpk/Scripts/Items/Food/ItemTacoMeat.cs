namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemTacoMeat : ProtoItemFood
    {
        public override string Description =>
            "Crispy tortilla shell filled with meat, special sauce and a bit of veggies.";

        public override float FoodRestore => 20;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Taco con carne";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 25;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectSavoryFood>(intensity: 0.15);
        }
    }
}