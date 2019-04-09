namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemMeatRaw : ProtoItemFood, IProtoItemOrganic
    {
        public override string Description =>
            "Raw meat. Can be prepared in a variety of ways. Eating it raw is probably not a very good idea...";

        public override float FoodRestore => 5;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override string Name => "Raw meat";

        public override ushort OrganicValue => 10;

        public override float StaminaRestore => -100;

        protected override void ServerOnEat(ItemEatData data)
        {
            var character = data.Character;

            // 50% chance to get food poisoning unless you have artificial stomach
            if (!character.SharedHasPerk(StatName.PerkEatSpoiledFood)
                && RandomHelper.RollWithProbability(0.5))
            {
                character.ServerAddStatusEffect<StatusEffectNausea>(intensity: 0.75); // 7.5 minutes
            }

            base.ServerOnEat(data);
        }
    }
}