namespace AtomicTorch.CBND.CoreMod.Systems.CharacterStamina
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    /// <summary>
    /// Player character stamina system (used for running and other activity).
    /// </summary>
    public class CharacterStaminaSystem : ProtoSystem<CharacterStaminaSystem>
    {
        private const double StaminaRegenerationWhenMovingMultiplier = 0.5;

        public static void ServerNotifyClientStaminaChange(ICharacter character, float deltaStamina)
        {
            //Logger.Dev($"{character}: sending stamina change from server: {deltaStamina:F2}");
            Instance.CallClient(character, _ => _.ClientRemote_StaminaChange(deltaStamina));
        }

        /// <summary>
        /// Shared (current character on client, server) update of
        /// player character's stamina (every frame, called directly from PlayerCharacter).
        /// </summary>
        public static void SharedUpdate(
            ICharacter character,
            PlayerCharacterPublicState publicState,
            PlayerCharacterPrivateState privateState,
            double deltaTime)
        {
            if (!character.ServerIsOnline
                || privateState.IsDespawned
                || publicState.IsDead)
            {
                return;
            }

            var stats = publicState.CurrentStatsExtended;

            // restore stamina slower when no food or water
            var staminaRestoreMultiplier = stats.FoodCurrent <= 0 || stats.WaterCurrent <= 0
                                               ? 0.33
                                               : 1;

            var finalStatsCache = privateState.FinalStatsCache;
            var statStaminaRegeneration = finalStatsCache[StatName.StaminaRegenerationPerSecond];

            // update character stats
            if (publicState.AppliedInput.MoveModes != CharacterMoveModes.None
                && publicState.CurrentVehicle is null)
            {
                if ((publicState.AppliedInput.MoveModes & CharacterMoveModes.ModifierRun) != 0)
                {
                    // character is running - consume stamina
                    var staminaConsumption = finalStatsCache[StatName.RunningStaminaConsumptionPerSecond]
                                             * deltaTime;
                    if (staminaConsumption > 0)
                    {
                        stats.SharedSetStaminaCurrent((float)(stats.StaminaCurrent - staminaConsumption),
                                                      notifyClient: false);
                    }

                    if (Api.IsServer)
                    {
                        // TODO: check if character really moved and not running into a wall
                        privateState.Skills.ServerAddSkillExperience<SkillAthletics>(
                            SkillAthletics.ExperienceAddWhenRunningPerSecond * deltaTime);
                    }
                }
                else // if (!privateState.Input.MoveModes.HasFlag(CharacterMoveModes.ModifierRun))
                {
                    // character is moving but not running (and client is not requested running) - restore stamina at special rate
                    var staminaRestore = StaminaRegenerationWhenMovingMultiplier
                                         * statStaminaRegeneration
                                         * staminaRestoreMultiplier
                                         * deltaTime;
                    stats.SharedSetStaminaCurrent((float)(stats.StaminaCurrent + staminaRestore), notifyClient: false);
                }
            }
            else
            {
                // character is staying or in a vehicle - restore stamina
                var staminaRestore = statStaminaRegeneration * staminaRestoreMultiplier * deltaTime;
                stats.SharedSetStaminaCurrent((float)(stats.StaminaCurrent + staminaRestore), notifyClient: false);
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableOrdered)]
        private void ClientRemote_StaminaChange(float deltaStamina)
        {
            var stats = ClientCurrentCharacterHelper.PublicState.CurrentStatsExtended;
            stats.SharedSetStaminaCurrent(stats.StaminaCurrent + deltaStamina, notifyClient: false);
            //Logger.Dev("Received stamina change from server: " + deltaStamina.ToString("F2"));
        }
    }
}