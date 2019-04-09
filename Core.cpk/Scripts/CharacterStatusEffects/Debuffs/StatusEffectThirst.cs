namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class StatusEffectThirst : ProtoStatusEffect
    {
        private const double DamagePerSecond = 0.4;

        public override string Description => "You are suffering from severe dehydration!";

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Thirst";

        public override double ServerAutoAddRepeatIntervalSeconds => 1;

        protected override void ClientDeinitialize(StatusEffectData data)
        {
            ClientComponentStatusEffectThirstManager.Intensity = 0;
        }

        protected override void ClientUpdate(StatusEffectData data)
        {
            ClientComponentStatusEffectThirstManager.Intensity = data.Intensity;
        }

        protected override void PrepareEffects(Effects effects)
        {
            // no health regeneration while thirsty
            effects.AddPercent(this, StatName.HealthRegenerationPerSecond, -100);
        }

        protected override IEnumerable<ICharacter> ServerAutoAddGetCharacterCandidates()
        {
            return Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
        }

        protected override void ServerOnAutoAdd(ICharacter character)
        {
            if (!character.SharedHasStatusEffect<StatusEffectThirst>()
                && IsLowWater(character))
            {
                // character becomes thirsty
                character.ServerAddStatusEffect(this, intensity: 1);
            }
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            var character = data.Character;
            if (!IsLowWater(character))
            {
                // not thirsty anymore
                character.ServerRemoveStatusEffect(this);
                return;
            }

            // reduce character health
            var stats = data.CharacterCurrentStats;
            var newHealth = stats.HealthCurrent - DamagePerSecond * data.DeltaTime;

            stats.ServerSetHealthCurrent((float)newHealth);
        }

        private static bool IsLowWater(ICharacter character)
        {
            var stats = character.GetPublicState<PlayerCharacterPublicState>().CurrentStatsExtended;
            var waterFraction = stats.WaterCurrent / stats.WaterMax;
            return waterFraction < 0.001;
        }
    }
}