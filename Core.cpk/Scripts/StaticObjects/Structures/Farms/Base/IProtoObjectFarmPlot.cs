namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms
{
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoObjectFarmPlot : IProtoObjectFarm
    {
        TextureAtlasResource BlendMaskTextureAtlas { get; }

        ITextureResource Texture { get; }
    }
}