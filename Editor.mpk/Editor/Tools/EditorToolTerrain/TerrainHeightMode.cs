namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrain
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum TerrainHeightMode : byte
    {
        Increase,

        Keep,

        Flatten,

        Decrease
    }
}