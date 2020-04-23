namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.GameEngine.Common.Helpers;

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

        protected override void ServerOnEat(ItemEatData data)
        {
            var character = data.Character;

            // 33% chance to get food poisoning unless you have artificial stomach
            if (!character.SharedHasPerk(StatName.PerkEatSpoiledFood)
                && RandomHelper.RollWithProbability(0.33))
            {
                character.ServerAddStatusEffect<StatusEffectNausea>(intensity: 0.5); // 5 minutes
            }

            base.ServerOnEat(data);
        }
    }
}