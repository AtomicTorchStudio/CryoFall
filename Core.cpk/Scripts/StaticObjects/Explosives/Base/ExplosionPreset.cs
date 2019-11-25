namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special.Base;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ExplosionPreset
    {
        public readonly double BlastwaveAnimationDuration;

        public readonly Color BlastWaveColor;

        public readonly double BlastwaveDelay;

        public readonly Size2F BlastwaveSizeFrom;

        public readonly Size2F BlastwaveSizeTo;

        public readonly Color LightColor;

        public readonly double LightDuration;

        public readonly double LightWorldSize;

        public readonly ProtoObjectCharredGround ProtoObjectCharredGround;

        public readonly float ScreenShakesDuration;

        public readonly float ScreenShakesWorldDistanceMax;

        public readonly float ScreenShakesWorldDistanceMin;

        public readonly double ServerDamageApplyDelay;

        public readonly ReadOnlySoundResourceSet SoundSet;

        public readonly double SpriteAnimationDuration;

        public readonly IReadOnlyList<TextureAtlasResource> SpriteAtlasResources;

        public readonly float SpriteBrightness;

        public readonly Color? SpriteColorAdditive;

        public readonly Color SpriteColorMultiplicative;

        public readonly Size2F SpriteSize;

        public ExplosionPreset(
            ProtoObjectCharredGround protoObjectCharredGround,
            double serverDamageApplyDelay,
            TextureAtlasResource[] spriteAtlasResources,
            ReadOnlySoundResourceSet soundSet,
            Color? spriteColorAdditive,
            Color? spriteColorMultiplicative,
            double spriteBrightness,
            double spriteAnimationDuration,
            Size2F spriteSize,
            double blastwaveDelay,
            double blastwaveAnimationDuration,
            Color blastWaveColor,
            Size2F blastwaveSizeFrom,
            Size2F blastwaveSizeTo,
            double lightWorldSize,
            double lightDuration,
            Color lightColor,
            float screenShakesDuration,
            float screenShakesWorldDistanceMin,
            float screenShakesWorldDistanceMax)
        {
            this.ServerDamageApplyDelay = serverDamageApplyDelay;
            this.SpriteAtlasResources = spriteAtlasResources;
            this.SoundSet = soundSet;
            this.SpriteColorAdditive = spriteColorAdditive;
            this.SpriteColorMultiplicative = spriteColorMultiplicative ?? Colors.White;
            this.SpriteBrightness = (float)spriteBrightness;
            this.SpriteAnimationDuration = spriteAnimationDuration;
            this.SpriteSize = spriteSize;

            this.BlastwaveDelay = blastwaveDelay;
            this.BlastwaveAnimationDuration = blastwaveAnimationDuration;
            this.BlastWaveColor = blastWaveColor;
            this.BlastwaveSizeFrom = blastwaveSizeFrom;
            this.BlastwaveSizeTo = blastwaveSizeTo;

            this.LightWorldSize = lightWorldSize;
            this.LightDuration = lightDuration;
            this.LightColor = lightColor;

            this.ScreenShakesDuration = screenShakesDuration;
            this.ScreenShakesWorldDistanceMin = screenShakesWorldDistanceMin;
            this.ScreenShakesWorldDistanceMax = screenShakesWorldDistanceMax;
            this.ProtoObjectCharredGround = protoObjectCharredGround;

            if (this.SoundSet.Count == 0)
            {
                Api.Logger.Warning("No sounds in the explosion sounds preset - please check the sounds path");
            }
        }

        // helper method to create and setup the explosion preset
        public static ExplosionPreset CreatePreset(
            ProtoObjectCharredGround protoObjectCharredGround,
            double serverDamageApplyDelay,
            string soundSetPath,
            double spriteAnimationDuration,
            string spriteSetPath,
            byte spriteAtlasColumns,
            byte spriteAtlasRows,
            Size2F spriteWorldSize,
            double blastwaveDelay,
            double blastwaveAnimationDuration,
            Color blastWaveColor,
            Size2F blastwaveWorldSizeFrom,
            Size2F blastwaveWorldSizeTo,
            double lightDuration,
            double lightWorldSize,
            Color lightColor,
            double screenShakesDuration,
            double screenShakesWorldDistanceMin,
            double screenShakesWorldDistanceMax,
            Color? spriteColorAdditive = null,
            Color? spriteColorMultiplicative = null,
            double spriteBrightness = 1)
        {
            var sounds = new SoundResourceSet()
                         .Add(soundSetPath)
                         .ToReadOnly();

            using var tempFilePaths = Api.Shared.FindFilesWithTrailingNumbers(
                ContentPaths.Textures + spriteSetPath);
            var filePaths = tempFilePaths.AsList();
            if (filePaths.Count == 0)
            {
                Api.Logger.Error("The explosion preset is empty - no explosion textures found at "
                                 + spriteSetPath);
            }

            var spriteAtlasResources = new TextureAtlasResource[filePaths.Count];
            for (var index = 0; index < filePaths.Count; index++)
            {
                var filePath = filePaths[index];
                spriteAtlasResources[index] = new TextureAtlasResource(filePath,
                                                                       spriteAtlasColumns,
                                                                       spriteAtlasRows,
                                                                       isTransparent: true);
            }

            return new ExplosionPreset(protoObjectCharredGround,
                                       serverDamageApplyDelay,
                                       spriteAtlasResources,
                                       sounds,
                                       spriteColorAdditive,
                                       spriteColorMultiplicative,
                                       spriteBrightness,
                                       spriteAnimationDuration,
                                       spriteWorldSize * ScriptingConstants.TileSizeRealPixels,
                                       blastwaveDelay,
                                       blastwaveAnimationDuration,
                                       blastWaveColor,
                                       blastwaveWorldSizeFrom * ScriptingConstants.TileSizeRealPixels,
                                       blastwaveWorldSizeTo * ScriptingConstants.TileSizeRealPixels,
                                       lightWorldSize,
                                       lightDuration,
                                       lightColor,
                                       (float)screenShakesDuration,
                                       (float)screenShakesWorldDistanceMin,
                                       (float)screenShakesWorldDistanceMax);
        }
    }
}