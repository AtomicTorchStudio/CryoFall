namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemUkha : ProtoItemFood
    {
        public override string Description =>
            "Clear soup with fish and vegetables in traditional style.";

        public override float FoodRestore => 20;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Lasting;

        public override string Name => "Ukha";

        public override ushort OrganicValue => 5;

        public override float WaterRestore => 10;

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectHealthyFood>(intensity: 0.40);
        }
    }
}