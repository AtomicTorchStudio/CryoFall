namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Resources;

    public static class ClientCharacterSkinTexturesCache
    {
        private static readonly Dictionary<(TextureResource, string), ProceduralTexture> Cache
            = new();

        public static ProceduralTexture Get(TextureResource textureResource, string skinToneId)
        {
            var key = (textureResource, skinToneId);
            if (Cache.TryGetValue(key, out var proceduralTexture))
            {
                return proceduralTexture;
            }

            proceduralTexture = new ProceduralTexture(
                "Colored skin sprite skinToneId="
                + skinToneId
                + " path="
                + textureResource.FullPath,
                proceduralTextureRequest
                    => ClientSpriteLutColorRemappingHelper.GetColorizedSprite(
                        proceduralTextureRequest,
                        textureResource,
                        SharedCharacterFaceStylesProvider.GetSkinToneFilePath(skinToneId),
                        spriteQualityOffset: textureResource.QualityOffset),
                isTransparent: true,
                isUseCache: false);

            Cache[key] = proceduralTexture;
            return proceduralTexture;
        }
    }
}