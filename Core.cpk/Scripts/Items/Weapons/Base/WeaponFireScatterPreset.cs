namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    /// <summary>
    /// Configure projectiles per shot.
    /// </summary>
    public readonly struct WeaponFireScatterPreset
    {
        private static readonly double[] DefaultProjectileAngleOffets = { 0 };

        private readonly double[] projectileAngleOffets;

        /// <param name="projectileAngleOffets">
        /// Angle offsets of each projectile per shot
        /// (for shotgun and other weapons firing several projectiles per shot).
        /// Null by default (single projectile, zero offset).
        /// </param>
        public WeaponFireScatterPreset(double[] projectileAngleOffets)
        {
            this.projectileAngleOffets = projectileAngleOffets;
        }

        public double[] ProjectileAngleOffets => this.projectileAngleOffets
                                                 ?? DefaultProjectileAngleOffets;
    }
}