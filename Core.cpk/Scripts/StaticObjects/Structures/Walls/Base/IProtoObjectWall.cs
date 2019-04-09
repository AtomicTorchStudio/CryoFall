namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoObjectWall : IProtoObjectStructure
    {
        TextureAtlasResource TextureAtlasDestroyed { get; }

        TextureAtlasResource TextureAtlasPrimary { get; }
    }
}