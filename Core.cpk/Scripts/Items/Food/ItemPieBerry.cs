namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;

    public class ItemPieBerry : ProtoItemFood
    {
        public override string Description => "Juicy-looking berry pie. Just like your grandma used to make.";

        public override float FoodRestore => 25;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Berry pie";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => 25;

        public override float WaterRestore => 5;

        protected override void ServerOnEat(ItemEatData data)
        {
            data.Character.ServerAddStatusEffect<StatusEffectWellFed>(intensity: 0.4);

            base.ServerOnEat(data);
        }
    }
}