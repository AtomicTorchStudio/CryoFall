namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using AtomicTorch.CBND.CoreMod.Systems.PvE;

    public readonly struct PvPTechTimeGateDurations
    {
        public PvPTechTimeGateDurations(double[] timeGates)
        {
            this.TimeGates = timeGates;
        }

        public double[] TimeGates { get; }

        public double Get(TechTier tier, bool isSpecialized)
        {
            if (PveSystem.SharedIsPve(false))
            {
                return 0;
            }

            var tierIndex = (int)tier - (int)TechConstants.PvPTimeGateStartsFromTier;
            if (tierIndex < 0)
            {
                return 0;
            }

            var index = 2 * tierIndex + (isSpecialized ? 1 : 0);
            return this.TimeGates[index];
        }
    }
}