namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using System.Linq;
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
            TextureAtlasResource textureAtlasResource,
            Color? lightColor = null,
            bool useScreenBlending = false)
        {
            var frames = ClientComponentSpriteSheetAnimator.CreateAnimationFrames(textureAtlasResource);
            this.HitSparksPreset[material] = new HitSparksEntry(frames, lightColor, useScreenBlending);
            return this;
        }

        public WeaponHitSparksPreset Clone()
        {
            return new WeaponHitSparksPreset(this);
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
            TextureAtlasResource textureAtlasResource,
            Color? lightColor = null,
            bool useScreenBlending = false)
        {
            var frames = ClientComponentSpriteSheetAnimator.CreateAnimationFrames(textureAtlasResource);
            foreach (var soundMaterial in AllSoundMaterials)
            {
                this.HitSparksPreset[soundMaterial] = new HitSparksEntry(frames, lightColor, useScreenBlending);
            }

            return this;
        }

        public readonly struct HitSparksEntry
        {
            public readonly Color? LightColor;

            public readonly ITextureResource[] SpriteSheetAnimationFrames;

            public readonly bool UseScreenBlending;

            public HitSparksEntry(
                ITextureResource[] spriteSheetAnimationFrames,
                Color? lightColor,
                bool useScreenBlending)
            {
                this.SpriteSheetAnimationFrames = spriteSheetAnimationFrames;
                this.LightColor = lightColor;
                this.UseScreenBlending = useScreenBlending;
            }
        }
    }
}