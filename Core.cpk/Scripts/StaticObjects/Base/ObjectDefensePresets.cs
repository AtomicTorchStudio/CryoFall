namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    public static class ObjectDefensePresets
    {
        // TODO: move this constant somewhere else? It's also used for damage calculations.
        public const double Max = 10000.0;

        /// <summary>
        /// Default armor tier. No protection.
        /// </summary>
        public static readonly ReadOnlyDefenseDescription Default
            = new DefenseDescription().Set(
                                          chemical: 0,
                                          cold: 0,
                                          electrical: 0,
                                          heat: 0,
                                          impact: 0,
                                          kinetic: 0,
                                          psi: Max,
                                          radiation: Max)
                                      .ToReadOnly();

        /// <summary>
        /// Should be used for things like wooden structures and such, that have weak defense.
        /// </summary>
        public static readonly ReadOnlyDefenseDescription Tier1
            = new DefenseDescription().Set(
                                          chemical: 0.33,
                                          cold: 0.33,
                                          electrical: 0.33,
                                          heat: 0.33,
                                          impact: 0.33,
                                          kinetic: 0.33,
                                          psi: Max,
                                          radiation: Max)
                                      .ToReadOnly();

        /// <summary>
        /// Should be used for stone or similar tier structures that have average protection.
        /// </summary>
        public static readonly ReadOnlyDefenseDescription Tier2
            = new DefenseDescription().Set(
                                          chemical: 0.66,
                                          cold: 0.66,
                                          electrical: 0.66,
                                          heat: 0.66,
                                          impact: 0.66,
                                          kinetic: 0.66,
                                          psi: Max,
                                          radiation: Max)
                                      .ToReadOnly();

        /// <summary>
        /// Should be used for most metal structures and otherwise highly armored structures.
        /// </summary>
        public static readonly ReadOnlyDefenseDescription Tier3
            = new DefenseDescription().Set(
                                          chemical: 1.0,
                                          cold: 1.0,
                                          electrical: 1.0,
                                          heat: 1.0,
                                          impact: 1.0,
                                          kinetic: 1.0,
                                          psi: Max,
                                          radiation: Max)
                                      .ToReadOnly();

        /// <summary>
        /// Should be used for steel or other very heavily armored structures and this is currently the highest "normal" tier of
        /// defense.
        /// </summary>
        public static readonly ReadOnlyDefenseDescription Tier4
            = new DefenseDescription().Set(
                                          chemical: 2.0,
                                          cold: 2.0,
                                          electrical: 2.0,
                                          heat: 2.0,
                                          impact: 2.0,
                                          kinetic: 2.0,
                                          psi: Max,
                                          radiation: Max)
                                      .ToReadOnly();

        /// <summary>
        /// Requires 50 or more damage per attack to penetrate.
        /// Should be used for heavily armored structures that cannot be damaged by most normal weapons.
        /// </summary>
        public static readonly ReadOnlyDefenseDescription Tier5
            = new DefenseDescription().Set(
                                          chemical: 5,
                                          cold: 5,
                                          electrical: 5,
                                          heat: 5,
                                          impact: 5,
                                          kinetic: 5,
                                          psi: Max,
                                          radiation: Max)
                                      .ToReadOnly();

        /// <summary>
        /// Should be used for things that we intent to be "immortal" (made from "adminium").
        /// </summary>
        public static readonly ReadOnlyDefenseDescription Impenetrable
            = new DefenseDescription().Set(
                                          chemical: Max,
                                          cold: Max,
                                          electrical: Max,
                                          heat: Max,
                                          impact: Max,
                                          kinetic: Max,
                                          psi: Max,
                                          radiation: Max)
                                      .ToReadOnly();
    }
}