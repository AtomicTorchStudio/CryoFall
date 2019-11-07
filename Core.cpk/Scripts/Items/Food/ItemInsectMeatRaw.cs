namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemInsectMeatRaw : ProtoItemFood, IProtoItemOrganic
    {
        public override string Description =>
            "Disgusting insect meat. Maybe cooking it will make it slightly less disgusting.";

        public override float FoodRestore => 3;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Perishable;

        public override string Name => "Raw insect meat";

        public override ushort OrganicValue => 5;

        public override float StaminaRestore => -100;

        protected override void ServerOnEat(ItemEatData data)
        {
            var character = data.Character;

            // get food poisoning unless you have artificial stomach
            if (!character.SharedHasPerk(StatName.PerkEatSpoiledFood))
            {
                character.ServerAddStatusEffect<StatusEffectNausea>(intensity: 0.75); // 7.5 minutes
            }

            base.ServerOnEat(data);
        }
    }
}