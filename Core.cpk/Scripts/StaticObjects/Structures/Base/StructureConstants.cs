namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures
{
    public static class StructureConstants
    {
        /// <summary>
        /// You can enable/disable the structures decay in Editor.
        /// </summary>
        public const bool IsStructureDecayEnabledInEditor = false;

        /// <summary>
        /// Time (in real world) before a structure will start decay.
        /// </summary>
        public const double StructureDecayDelaySeconds = 32 * 60 * 60; // 32 hours

        /// <summary>
        /// Time (in real world) for a structure to completely decay (destroy).
        /// While decaying, the structure will receive a regular damage proportional to its HP.
        /// It also means that if the structure is already damaged, it will decay even faster.
        /// </summary>
        public const double StructureDecayDurationSeconds = 24 * 60 * 60; // 24 hours

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
    }
}