namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures
{
    public static class StructureConstants
    {
        /// <summary>
        /// Time (in real world seconds) before an abandoned structure (without the land claim) will start decaying.
        /// Default value: 32 hours (115200 seconds). Don't set it higher than 2 billions.
        /// </summary>
        public const double DecayDelaySeconds = 32 * 60 * 60;

        /// <summary>
        /// Time (in real world seconds) for a structure to completely destroy by decay.
        /// While decaying, the structure will receive a regular damage proportional to its HP.
        /// It also means that if the structure is already damaged, it will decay even faster.
        /// Default value: 24 hours (86400 seconds). Don't set it higher than 2 billions.
        /// </summary>
        public const double DecayDurationSeconds = 24 * 60 * 60;

        /// <summary>
        /// Interval with which the decay system will process the objects decay.
        /// Should not be too short as it will reduce the structure points too often
        /// (causing unnecessary network replication, spending more CPU time, and also having granularity issues).
        /// Should not be too large as it will reduce the precision.
        /// </summary>
        public const double DecaySystemUpdateIntervalSeconds = 5 * 60; // every 5 minutes
    }
}