namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public interface IProtoTileWater : IProtoTile
    {
        IProtoTileWater BridgeProtoTile { get; }

        bool CanCollect { get; }

        bool IsFishingAllowed { get; }

        TextureResource UnderwaterGroundTextureAtlas { get; }

        RenderingMaterial ClientGetWaterBlendMaterial(ClientTileBlendHelper.BlendLayer blendLayer);

        RenderingMaterial ClientGetWaterPrimaryMaterial();
    }
}