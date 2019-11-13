namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Resources;

    public class WeaponFireTracePreset
    {
        public readonly IReadOnlyWeaponHitSparksPreset HitSparksPreset;

        public readonly double TraceEndFadeOutExponent;

        public readonly double TraceSpeed;

        public readonly double TraceStartScaleSpeedExponent;

        public readonly double TraceStartWorldOffset;

        public readonly ITextureResource TraceTexture;

        public readonly double TraceWorldLength;

        public WeaponFireTracePreset(
            string traceTexturePath,
            IReadOnlyWeaponHitSparksPreset hitSparksPreset,
            double traceSpeed,
            ushort traceSpriteWidthPixels,
            int traceStartOffsetPixels,
            double traceStartScaleSpeedExponent = 1.0,
            double traceEndFadeOutExponent = 1.0)
        {
            this.TraceTexture = new TextureResource(traceTexturePath, isTransparent: true);
            this.TraceSpeed = traceSpeed;
            this.HitSparksPreset = hitSparksPreset;

            this.TraceStartScaleSpeedExponent = traceStartScaleSpeedExponent;
            this.TraceEndFadeOutExponent = traceEndFadeOutExponent;

            this.TraceWorldLength = traceSpriteWidthPixels / 256.0;
            this.TraceStartWorldOffset = -traceStartOffsetPixels / 256.0;
        }

        public WeaponHitSparksPreset.HitSparksEntry GetHitSparksEntry(ObjectMaterial material)
        {
            return this.HitSparksPreset.GetForMaterial(material);
        }
    }
}