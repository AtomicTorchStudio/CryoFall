namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public static class WeaponConstants
    {
        public static double DamageByCreaturesMultiplier
            => RateDamageByCreaturesMultiplier.SharedValue;

        public static double DamageExplosivesToCharactersMultiplier
            => RateDamageExplosivesToCharactersMultiplier.GetSharedValue(logErrorIfClientHasNoValue: false);

        public static double DamageExplosivesToStructuresMultiplier
            => RateDamageExplosivesToStructuresMultiplier.GetSharedValue(logErrorIfClientHasNoValue: false);

        public static double DamageFriendlyFireMultiplier
            => RateDamageFriendlyFireMultiplier.SharedValue;

        public static double DamagePveMultiplier
            => RateDamagePvEMultiplier.SharedValue;

        /// <summary>
        /// Please note: we're using 0.5 multiplier here as this is how PvP damage was balanced.
        /// Making it x1.0 is not reasonable as it will require completely reworking PvE damage balance
        /// which is not viable considering how the damage calculation formula works.
        /// </summary>
        public static double DamagePvpMultiplier
            => 0.5 * RatePvPDamageMultiplier.SharedValue;
    }
}