namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemInsectMeatFried : ProtoItemFood
    {
        public override string Description =>
            "Cooking this meat made it slightly less disgusting... But be prepared to vomit if you decide to eat it.";

        public override float FoodRestore => 7;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override string Name => "Fried insect meat";

        public override ushort OrganicValue => 5;

        protected override void ServerOnEat(ItemEatData data)
        {
            var character = data.Character;

            // 25% chance to get food poisoning unless you have artificial stomach
            if (!character.SharedHasPerk(StatName.PerkEatSpoiledFood)
                && RandomHelper.RollWithProbability(0.25))
            {
                character.ServerAddStatusEffect<StatusEffectNausea>(intensity: 0.25); // 2.5 minutes
            }

            base.ServerOnEat(data);
        }
    }
}