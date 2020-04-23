namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    public static class WeaponFireTracePresets
    {
        public static readonly WeaponFireTracePreset Arrow
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TraceArrow",
                hitSparksPreset: WeaponHitSparksPresets.Firearm,
                traceSpeed: 19,
                traceSpriteWidthPixels: 250,
                traceStartOffsetPixels: -10);

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

        public static readonly WeaponFireTracePreset Grenade
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TraceGrenade",
                hitSparksPreset: WeaponHitSparksPresets.Firearm,
                traceSpeed: 20,
                traceSpriteWidthPixels: 169,
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
                traceTexturePath: null,
                hitSparksPreset: WeaponHitSparksPresets.NoWeapon,
                traceSpeed: 1,
                traceSpriteWidthPixels: 0,
                traceStartOffsetPixels: 0);

        public static readonly WeaponFireTracePreset MobPoison
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TracePoison",
                hitSparksPreset: WeaponHitSparksPresets.NoWeapon,
                traceSpeed: 20,
                traceSpriteWidthPixels: 362,
                traceStartScaleSpeedExponent: 0.5,
                traceStartOffsetPixels: -17);

        public static readonly WeaponFireTracePreset MobPragmiumQueen
            = new WeaponFireTracePreset(
                traceTexturePath: "FX/WeaponTraces/TraceMobWeaponPragmiumQueen",
                hitSparksPreset: WeaponHitSparksPresets.Plasma,
                traceSpeed: 20,
                traceSpriteWidthPixels: 362,
                traceStartScaleSpeedExponent: 0.5,
                traceStartOffsetPixels: -33);

        public static readonly WeaponFireTracePreset NoWeapon
            = new WeaponFireTracePreset(
                traceTexturePath: null,
                hitSparksPreset: WeaponHitSparksPresets.NoWeapon,
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