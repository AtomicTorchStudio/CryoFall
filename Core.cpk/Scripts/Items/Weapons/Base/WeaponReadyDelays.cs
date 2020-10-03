namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    public static class WeaponReadyDelays
    {
        /// <summary>
        /// Pistols are balanced such that allow their use when your main weapon has run out of ammo.
        /// So switching is pretty much instant. Applies to conventional pistols only.
        /// </summary>
        public const double ConventionalPistols = 0.3;

        /// <summary>
        /// Used for all melee weapons by default.
        /// </summary>
        public const double DefaultMelee = 0.5;

        /// <summary>
        /// Used for all ranged weapons by default.
        /// </summary>
        public const double DefaultRanged = 1.0;
    }
}