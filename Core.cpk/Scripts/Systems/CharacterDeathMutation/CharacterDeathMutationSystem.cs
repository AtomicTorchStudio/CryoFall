namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDeathMutation
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    /// <summary>
    /// Applies mutation in case of player character death from radiation.
    /// </summary>
    public class CharacterDeathMutationSystem : ProtoSystem<CharacterDeathMutationSystem>
    {
        public override string Name => "Character death mutation system";

        protected override void PrepareSystem()
        {
            base.PrepareSystem();

            if (IsServer)
            {
                ServerCharacterDeathMechanic.CharacterDeath += ServerCharacterDeathHandler;
            }
        }

        private static void ServerCharacterDeathHandler(ICharacter character)
        {
            if (character.IsNpc)
            {
                return;
            }

            // Mutation status effect is added upon death if EITHER one of the two conditions are met:
            // player had 15% or more radiation poisoning when they died
            // OR they had 15% or more damage received from radiation when they died.
            if (ServerIsDeathFromRadiation())
            {
                Logger.Info("Character died from radiation, adding mutation", character);
                character.ServerAddStatusEffect<StatusEffectMutation>(intensity: 1.0);
            }

            bool ServerIsDeathFromRadiation()
            {
                if (character.SharedGetStatusEffectIntensity<StatusEffectRadiationPoisoning>()
                    > 0.15)
                {
                    return true;
                }

                var damageSources = CharacterDamageTrackingSystem.ServerGetDamageSources(character);
                if (damageSources == null)
                {
                    return false;
                }

                foreach (var damageSource in damageSources)
                {
                    if (damageSource.ProtoEntity is StatusEffectRadiationPoisoning
                        && damageSource.Fraction > 0.15)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}