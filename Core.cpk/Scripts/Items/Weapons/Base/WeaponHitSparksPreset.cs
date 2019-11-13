namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Resources;
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
            Color? lightColor = null)
        {
            var frames = ClientComponentSpriteSheetAnimator.CreateAnimationFrames(textureAtlasResource);
            this.HitSparksPreset[material] = new HitSparksEntry(frames, lightColor);
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

        public WeaponHitSparksPreset SetDefault(
            TextureAtlasResource textureAtlasResource,
            Color? lightColor = null)
        {
            var frames = ClientComponentSpriteSheetAnimator.CreateAnimationFrames(textureAtlasResource);
            foreach (var soundMaterial in AllSoundMaterials)
            {
                this.HitSparksPreset[soundMaterial] = new HitSparksEntry(frames, lightColor);
            }

            return this;
        }

        public readonly struct HitSparksEntry
        {
            public readonly Color? LightColor;

            public readonly ITextureResource[] SpriteSheetAnimationFrames;

            public HitSparksEntry(ITextureResource[] spriteSheetAnimationFrames, Color? lightColor)
            {
                this.SpriteSheetAnimationFrames = spriteSheetAnimationFrames;
                this.LightColor = lightColor;
            }
        }
    }
}