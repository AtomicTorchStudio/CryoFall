namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class ItemMushroomPink : ProtoItemFood
    {
        public override string Description =>
            "Extremely toxic mushroom. So toxic it glows at night.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Lasting;

        public override string Name => "Pink mushroom";

        public override ushort OrganicValue => 5;

        // shouldn't be eaten
        protected override void ServerOnEat(ItemEatData data)
        {
            var character = data.Character;

            // 100% chance to get food poisoning unless you have artificial stomach
            if (!character.SharedHasPerk(StatName.PerkEatSpoiledFood))
            {
                // 1 intensity == 10 minutes
                character.ServerAddStatusEffect<StatusEffectNausea>(intensity: 1.0);
            }

            // 1 intensity == 5 minutes (basically 100% death)
            character.ServerAddStatusEffect<StatusEffectToxins>(intensity: 1.0);

            base.ServerOnEat(data);
        }
    }
}