namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    public static class WeaponFireTracePresets
    {
        public static readonly WeaponFireTracePreset Blackpowder
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TraceBlackpowder",
                hitSparksPreset: WeaponHitSparksPresets.Firearm,
                traceSpeed: 17,
                traceSpriteWidthPixels: 363,
                traceStartOffsetPixels: -10);

        public static readonly WeaponFireTracePreset Firearm
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TraceFirearm",
                hitSparksPreset: WeaponHitSparksPresets.Firearm,
                traceSpeed: 20,
                traceSpriteWidthPixels: 363,
                traceStartOffsetPixels: -10);

        public static readonly WeaponFireTracePreset Heavy
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TraceHeavy",
                hitSparksPreset: WeaponHitSparksPresets.Firearm,
                traceSpeed: 20,
                traceSpriteWidthPixels: 363,
                traceStartOffsetPixels: -10);

        public static readonly WeaponFireTracePreset HeavySniper
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TraceHeavy",
                hitSparksPreset: WeaponHitSparksPresets.Firearm,
                traceSpeed: 30,
                traceSpriteWidthPixels: 363,
                traceStartOffsetPixels: -10);

        public static readonly WeaponFireTracePreset Laser
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TraceLaser",
                hitSparksPreset: WeaponHitSparksPresets.Laser,
                traceSpeed: 22,
                traceSpriteWidthPixels: 363,
                traceStartScaleSpeedExponent: 0.5,
                traceStartOffsetPixels: -10,
                useScreenBlending: true);

        public static readonly WeaponFireTracePreset MeleeWeapon
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TraceFirearm", // not relevant for melee weapons
                hitSparksPreset: WeaponHitSparksPresets.Firearm,  // reuse firearm
                traceSpeed: 1,
                traceSpriteWidthPixels: 0,
                traceStartOffsetPixels: 0);

        public static readonly WeaponFireTracePreset NoWeapon
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TraceFirearm", // not relevant for melee weapons
                hitSparksPreset: WeaponHitSparksPresets.NoWeapon, // reuse firearm
                traceSpeed: 1,
                traceSpriteWidthPixels: 0,
                traceStartOffsetPixels: 0);

        public static readonly WeaponFireTracePreset Pellets
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TracePellets",
                hitSparksPreset: WeaponHitSparksPresets.Firearm,
                traceSpeed: 17,
                traceSpriteWidthPixels: 363,
                traceStartOffsetPixels: -10);

        public static readonly WeaponFireTracePreset Plasma
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TracePlasma",
                hitSparksPreset: WeaponHitSparksPresets.Plasma,
                traceSpeed: 20,
                traceSpriteWidthPixels: 235,
                traceStartScaleSpeedExponent: 0.5,
                traceStartOffsetPixels: -10,
                useScreenBlending: true);
    }
}