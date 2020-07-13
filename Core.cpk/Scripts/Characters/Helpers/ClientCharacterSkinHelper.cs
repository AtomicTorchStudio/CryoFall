namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Resources;

    public static class ClientCharacterSkinHelper
    {
        private static readonly Dictionary<(TextureResource, string), ProceduralTexture> Cache
            = new Dictionary<(TextureResource, string), ProceduralTexture>();

        public static ProceduralTexture GetSkinProceduralTexture(TextureResource textureResource, string skinToneId)
        {
            var key = (textureResource, skinToneId);
            if (Cache.TryGetValue(key, out var proceduralTexture))
            {
                return proceduralTexture;
            }

            proceduralTexture = new ProceduralTexture(
                "Colored skin sprite " + textureResource.FullPath,
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