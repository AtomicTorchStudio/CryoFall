namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    /// <summary>
    /// These constants are adjusting only extraction speed for the extractors.
    /// The fuel consumption is not changed.
    /// </summary>
    public static class ObjectExtractorConstants
    {
        public const double ExtractorPvE = 1.0;

        public const double ExtractorPvpWithDeposit = 2.0;

        public const double ExtractorPvpWithoutDeposit = 0.5;

        // Not used in A26.
        public const double InfiniteExtractorPvE = 1.0;

        // Not used in A26.
        public const double InfiniteExtractorPvP = 1.0;
    }
}