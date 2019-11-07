namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public static class ProtoCharacterExtensions
    {
        public static IDynamicWorldObject SharedGetCurrentVehicle(this ICharacter character)
        {
            return character.GetPublicState<PlayerCharacterPublicState>()
                            .CurrentVehicle;
        }

        /// <summary>
        /// Gets the stat multiplier from the FinalStatsCache of the character.
        /// </summary>
        public static double SharedGetFinalStatMultiplier(this ICharacter character, StatName statName)
        {
            return character.SharedGetFinalStatsCache()
                            .GetMultiplier(statName);
        }

        public static FinalStatsCache SharedGetFinalStatsCache(this ICharacter character)
        {
            return character.GetPrivateState<BaseCharacterPrivateState>()
                            .FinalStatsCache;
        }

        /// <summary>
        /// Gets the stat value from the FinalStatsCache of the character.
        /// </summary>
        public static double SharedGetFinalStatValue(this ICharacter character, StatName statName)
        {
            return character.SharedGetFinalStatsCache()[statName];
        }

        public static bool SharedHasPerk(this ICharacter character, StatName statName)
        {
            return character.SharedGetFinalStatsCache()[statName] >= 1;
        }
    }
}