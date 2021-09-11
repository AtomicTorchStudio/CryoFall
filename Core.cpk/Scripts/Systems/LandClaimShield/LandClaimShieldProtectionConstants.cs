namespace AtomicTorch.CBND.CoreMod.Systems.LandClaimShield
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public static class LandClaimShieldProtectionConstants
    {
        /// <summary>
        /// Shield activation duration.
        /// </summary>
        public static double SharedActivationDuration
            => RatePvPShieldProtectionActivationDuration.SharedValue;

        /// <summary>
        /// Cannot reactivate a deactivated shield for this duration.
        /// </summary>
        public static double SharedCooldownDuration
            => RatePvPShieldProtectionCooldownDuration.SharedValue;

        public static bool SharedIsEnabled
            => RatePvPShieldProtectionEnabled.SharedValue;
    }
}