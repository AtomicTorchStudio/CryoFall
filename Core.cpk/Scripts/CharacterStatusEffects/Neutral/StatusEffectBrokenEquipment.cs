namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class StatusEffectBrokenEquipment : ProtoStatusEffect
    {
        public override string Description =>
            "One or more of your equipment items has low durability and could break soon.";

        // does not decrease, it is purely informative effect
        public override double IntensityAutoDecreasePerSecondValue => 0;

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Broken equipment";

        public override double ServerAutoAddRepeatIntervalSeconds => 5;

        // becomes visible when reaches 80% (i.e. when any item durability is <= than 20%)
        public override double VisibilityIntensityThreshold => 1 - 0.2;

        protected override IEnumerable<ICharacter> ServerAutoAddGetCharacterCandidates()
        {
            return Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
        }

        protected override void ServerOnAutoAdd(ICharacter character)
        {
            if (character.SharedHasStatusEffect<StatusEffectBrokenEquipment>())
            {
                return;
            }

            var intensity = this.CalculateEffectIntensity(character);
            if (intensity > 0)
            {
                character.ServerAddStatusEffect(this, intensity);
            }
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            base.ServerUpdate(data);
            data.Intensity = this.CalculateEffectIntensity(data.Character);
        }

        private static double ServerGetLowestEquipmentDurabilityFraction(IEnumerable<IItem> items)
        {
            var lowestDurabilityFraction = 1.0;
            foreach (var item in items)
            {
                if (!(item.ProtoItem is IProtoItemEquipment protoItemEquipment))
                {
                    // not an equipment
                    continue;
                }

                switch (protoItemEquipment.EquipmentType)
                {
                    case EquipmentType.FullBody:
                    case EquipmentType.Head:
                    case EquipmentType.Chest:
                    case EquipmentType.Legs:
                        // consider these equipment item types for the durability check
                        break;

                    default:
                        // don't consider implants, devices and other (future?) slots
                        continue;
                }

                var durabilityFraction = ItemDurabilitySystem.SharedGetDurabilityFraction(item);
                if (durabilityFraction < lowestDurabilityFraction)
                {
                    lowestDurabilityFraction = durabilityFraction;
                }
            }

            return lowestDurabilityFraction;
        }

        private double CalculateEffectIntensity(ICharacter character)
        {
            var containerEquipment = character.SharedGetPlayerContainerEquipment();
            var lowestDurabilityFraction = ServerGetLowestEquipmentDurabilityFraction(containerEquipment.Items);
            // lowest durability fraction is in 0-1 range (closer to 0 - broken)
            var intensity = 1 - lowestDurabilityFraction;
            if (intensity < this.VisibilityIntensityThreshold)
            {
                intensity = 0;
            }

            return intensity;
        }
    }
}