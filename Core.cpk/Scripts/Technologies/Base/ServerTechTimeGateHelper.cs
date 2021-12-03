namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ServerTechTimeGateHelper
    {
        public static bool IsAvailableT3Basic(DropItemContext context)
        {
            return IsTimeGateFinished(TechTier.Tier3, isSpecialized: false);
        }

        public static bool IsAvailableT3Specialized(DropItemContext context)
        {
            return IsTimeGateFinished(TechTier.Tier3, isSpecialized: true);
        }

        public static bool IsAvailableT4Basic(DropItemContext context)
        {
            return IsTimeGateFinished(TechTier.Tier4, isSpecialized: false);
        }

        public static bool IsAvailableT4Specialized(DropItemContext context)
        {
            return IsTimeGateFinished(TechTier.Tier4, isSpecialized: true);
        }

        public static bool IsAvailableT5Basic(DropItemContext context)
        {
            return IsTimeGateFinished(TechTier.Tier5, isSpecialized: false);
        }

        public static bool IsAvailableT5Specialized(DropItemContext context)
        {
            return IsTimeGateFinished(TechTier.Tier5, isSpecialized: true);
        }

        private static double Get(TechTier tier, bool isSpecialized)
        {
            return TechConstants.PvPTimeGates.Get(tier, isSpecialized);
        }

        private static bool IsTimeGateFinished(TechTier tier, bool isSpecialized)
        {
            return IsTimeGateFinished(Get(tier, isSpecialized));
        }

        private static bool IsTimeGateFinished(double timeGateDuration)
        {
            if (Api.IsEditor)
            {
                return true;
            }

            if (PveSystem.ServerIsPvE)
            {
                // no time-gating in PvE
                return true;
            }

            var timeRemains = timeGateDuration - Api.Server.Game.SecondsSinceWorldCreation;
            return timeRemains <= 0;
        }
    }
}