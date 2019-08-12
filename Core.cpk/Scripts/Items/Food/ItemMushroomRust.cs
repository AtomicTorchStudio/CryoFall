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

        public override string Name => "Rustshroom";

        public override ushort OrganicValue => 5;

        protected override void ServerOnEat(ItemEatData data)
        {
            var character = data.Character;

            // shouldn't be eaten raw
            // 100% chance to get food poisoning unless you have artificial stomach
            if (!character.SharedHasPerk(StatName.PerkEatSpoiledFood))
            {
                character.ServerAddStatusEffect<StatusEffectNausea>(intensity: 0.5); // half duration
            }

            character.ServerAddStatusEffect<StatusEffectToxins>(intensity: 0.15); // short intoxication

            base.ServerOnEat(data);
        }
    }
}