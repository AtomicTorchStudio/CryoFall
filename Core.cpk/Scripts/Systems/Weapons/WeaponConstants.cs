namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public static class WeaponConstants
    {
        public static double DamageCreaturesMultiplier
            => RateDamageByCreaturesMultiplier.SharedValue;

        public static double DamageExplosivesToCharactersMultiplier
            => RateDamageExplosivesToCharactersMultiplier.GetSharedValue(logErrorIfClientHasNoValue: false);

        public static double DamageExplosivesToStructuresMultiplier
            => RateDamageExplosivesToStructuresMultiplier.GetSharedValue(logErrorIfClientHasNoValue: false);

        public static double DamageFriendlyFireMultiplier
            => RateDamageFriendlyFireMultiplier.SharedValue;

        public static double DamagePveMultiplier
            => RateDamagePvEMultiplier.SharedValue;

        public static double DamagePvpMultiplier
            => RatePvPDamageMultiplier.SharedValue;
    }
}