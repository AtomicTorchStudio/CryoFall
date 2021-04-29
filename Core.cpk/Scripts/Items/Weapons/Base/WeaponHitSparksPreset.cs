namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class WeaponHitSparksPreset : IReadOnlyWeaponHitSparksPreset
    {
        private static readonly ObjectMaterial[] AllSoundMaterials
            = EnumExtensions.GetValues<ObjectMaterial>();

        public readonly Dictionary<ObjectMaterial, HitSparksEntry> HitSparksPreset;

        public WeaponHitSparksPreset()
        {
            this.HitSparksPreset = new Dictionary<ObjectMaterial, HitSparksEntry>(
                capacity: AllSoundMaterials.Length);
        }

        private WeaponHitSparksPreset(WeaponHitSparksPreset cloneSource)
        {
            this.HitSparksPreset = new Dictionary<ObjectMaterial, HitSparksEntry>(
                cloneSource.HitSparksPreset);
        }

        public WeaponHitSparksPreset Add(
            ObjectMaterial material,
            double texturePivotY,
            TextureAtlasResource texture,
            Color? lightColor = null,
            bool useScreenBlending = false,
            bool allowRandomizedHitPointOffset = true)
        {
            var frames = ClientComponentSpriteSheetAnimator.CreateAnimationFrames(texture);
            this.HitSparksPreset[material] = new HitSparksEntry(frames,
                                                                lightColor,
                                                                useScreenBlending,
                                                                texturePivotY,
                                                                allowRandomizedHitPointOffset);
            return this;
        }

        public WeaponHitSparksPreset Clone()
        {
            return new(this);
        }

        public HitSparksEntry GetForMaterial(ObjectMaterial material)
        {
            return this.HitSparksPreset.Find(material);
        }

        public void PreloadTextures()
        {
            var rendering = Api.Client.Rendering;
            foreach (var entry in this.HitSparksPreset)
            {
                var frames = entry.Value.SpriteSheetAnimationFrames;
                if (frames.Length > 0)
                {
                    rendering.PreloadTextureAsync(frames[0]);
                }
            }
        }

        public WeaponHitSparksPreset SetDefault(
            double texturePivotY,
            TextureAtlasResource texture,
            Color? lightColor = null,
            bool useScreenBlending = false,
            bool allowRandomizedHitPointOffset = true)
        {
            var frames = ClientComponentSpriteSheetAnimator.CreateAnimationFrames(texture);
            foreach (var soundMaterial in AllSoundMaterials)
            {
                this.HitSparksPreset[soundMaterial] = new HitSparksEntry(frames,
                                                                         lightColor,
                                                                         useScreenBlending,
                                                                         texturePivotY,
                                                                         allowRandomizedHitPointOffset);
            }

            return this;
        }

        public readonly struct HitSparksEntry
        {
            public readonly bool AllowRandomizedHitPointOffset;

            public readonly Color? LightColor;

            public readonly double PivotY;

            public readonly ITextureResource[] SpriteSheetAnimationFrames;

            public readonly bool UseScreenBlending;

            public HitSparksEntry(
                ITextureResource[] spriteSheetAnimationFrames,
                Color? lightColor,
                bool useScreenBlending,
                double pivotY,
                bool allowRandomizedHitPointOffset)
            {
                this.AllowRandomizedHitPointOffset = allowRandomizedHitPointOffset;
                this.SpriteSheetAnimationFrames = spriteSheetAnimationFrames;
                this.LightColor = lightColor;
                this.UseScreenBlending = useScreenBlending;
                this.PivotY = pivotY;
            }
        }
    }
}