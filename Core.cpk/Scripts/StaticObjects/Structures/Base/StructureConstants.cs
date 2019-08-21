namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures
{
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class StructureConstants
    {
        // Please note that changing this value will require distribution
        // of a client+server mod so it will know about this change.
        public const byte BuildItemsCountMultiplier = 1;

        // Please note that changing this value will require distribution
        // of a client+server mod so it will know about this change.
        public const byte RepairItemsCountMultiplier = 1;

        /// <summary>
        /// The resource can be claimed only if it was spawned longer than defined here.
        /// </summary>
        public const int ResourceSpawnClaimingCooldownDuration = 30 * 60; // 30 minutes

        /// <summary>
        /// Refresh rate of the land claim objects for the decay reset check.
        /// The game server should check whether there are any land claim owners inside the land claim area
        /// and reset the decay timer if so.
        /// </summary>
        public const double StructureDecayLandClaimResetSystemUpdateIntervalSeconds = 5;

        /// <summary>
        /// Interval with which the decay system will process the objects decay.
        /// Should not be too short as it will reduce the structure points too often
        /// (causing unnecessary network replication, spending more CPU time, and also having granularity issues).
        /// Should not be too large as it will reduce the precision.
        /// </summary>
        public const double StructureDecaySystemUpdateIntervalSeconds = 5 * 60; // every 5 minutes

        public static readonly bool IsStructuresDecayEnabled;

        public static readonly double ManufacturingSpeedMultiplier;

        public static readonly double StructuresAbandonedDecayDelaySeconds;

        public static readonly double StructuresDecayDurationSeconds;

        public static readonly double StructuresLandClaimDecayDelayDurationMultiplier;

        static StructureConstants()
        {
            IsStructuresDecayEnabled =
                ServerRates.Get(
                    "StructuresDecayEnabled",
                    defaultValue: 1,
                    @"Set it to 0 to disable the structures decay.
                    Set it to 1 to enable the structures decay.")
                > 0;

            StructuresAbandonedDecayDelaySeconds = ServerRates.Get(
                "StructuresAbandonedDecayDelaySeconds",
                defaultValue: 32 * 60 * 60,
                @"Time (in real world seconds) before an abandoned structure (without the land claim) will start decaying.
                  Default value: 32 hours or 115200 seconds. Don't set it higher than 2 billions.");

            StructuresLandClaimDecayDelayDurationMultiplier = ServerRates.Get(
                "StructuresLandClaimDecayDelayDurationMultiplier",
                defaultValue: 1.0,
                @"Time multiplier before an abandoned land claim (or base) will start decaying.
                  For example, the default decay delay for the land claims (T1) is 32 hours,
                  but with 2.0 multiplier it will be increased to 64 hours (for T1).");

            StructuresDecayDurationSeconds = ServerRates.Get(
                "StructuresDecayDurationSeconds",
                defaultValue: 24 * 60 * 60,
                @"Time (in real world seconds) for a structure to completely destroy by decay.
                  While decaying, the structure will receive a regular damage proportional to its HP.
                  It also means that if the structure is already damaged, it will decay even faster.
                  Default value: 24 hours or 86400 seconds. Don't set it higher than 2 billions.");

            ManufacturingSpeedMultiplier = ServerRates.Get(
                "ManufacturingSpeedMultiplier",
                defaultValue: 1.0,
                @"Manufacturing rate for all manufacturers (such as furnaces, oil/Li extractors, oil refineries, wells, mulchboxes, etc.)");
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static void EnsureInitialized()
        {
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                EnsureInitialized();
            }
        }
    }
}