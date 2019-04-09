namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    internal static class SystemVegetation
    {
        public static ITextureResource ClientGetTexture(
            IProtoObjectVegetation protoObjectVegetation,
            IStaticWorldObject worldObject,
            VegetationPublicState publicState)
        {
            var textureResource = protoObjectVegetation.DefaultTexture;
            var textureAtlas = textureResource as TextureAtlasResource;
            if (textureAtlas == null)
            {
                // not a texture atlas - always use whole texture
                return textureResource;
            }

            var columnIndex = protoObjectVegetation.ClientGetTextureAtlasColumn(worldObject, publicState);
            return textureAtlas.Chunk(column: columnIndex, row: 0);
        }

        public static void ClientRefreshVegetationRendering(
            IProtoObjectVegetation protoObjectVegetation,
            IStaticWorldObject worldObject,
            VegetationClientState clientState,
            VegetationPublicState publicState)
        {
            clientState.LastGrowthStage = publicState.GrowthStage;
            clientState.Renderer.TextureResource = ClientGetTexture(protoObjectVegetation, worldObject, publicState);

            if (clientState.RendererShadow != null)
            {
                clientState.RendererShadow.Scale = protoObjectVegetation.CalculateShadowScale(clientState);
            }
        }
    }
}