namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class StatusEffectHunger : ProtoStatusEffect
    {
        private const double DamagePerSecond = 0.2;

        public override string Description => "You are starving to death!";

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Hunger";

        public override double ServerAutoAddRepeatIntervalSeconds => 1;

        protected override void ClientDeinitialize(StatusEffectData data)
        {
            ClientComponentStatusEffectHungerManager.Intensity = 0;
        }

        protected override void ClientUpdate(StatusEffectData data)
        {
            ClientComponentStatusEffectHungerManager.Intensity = data.Intensity;
        }

        protected override void PrepareEffects(Effects effects)
        {
            // no health regeneration while hungry
            effects.AddPercent(this, StatName.HealthRegenerationPerSecond, -100);
        }

        protected override IEnumerable<ICharacter> ServerAutoAddGetCharacterCandidates()
        {
            return Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
        }

        protected override void ServerOnAutoAdd(ICharacter character)
        {
            if (!character.SharedHasStatusEffect<StatusEffectHunger>()
                && IsLowFood(character))
            {
                // character becomes hungry
                character.ServerAddStatusEffect(this, intensity: 1);
            }
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            var character = data.Character;
            if (!character.IsOnline)
            {
                // only online characters are affected by hunger
                return;
            }

            if (!IsLowFood(character))
            {
                // not hungry anymore
                character.ServerRemoveStatusEffect(this);
                return;
            }

            // reduce character health
            var stats = data.CharacterCurrentStats;
            stats.ServerReduceHealth(DamagePerSecond * data.DeltaTime, this);
        }

        private static bool IsLowFood(ICharacter character)
        {
            var stats = character.GetPublicState<PlayerCharacterPublicState>().CurrentStatsExtended;
            var foodFraction = stats.FoodCurrent / stats.FoodMax;
            return foodFraction < 0.001;
        }
    }
}