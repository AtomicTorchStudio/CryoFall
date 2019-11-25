namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectWithAccessMode : IProtoWorldObject
    {
        bool IsClosedAccessModeAvailable { get; }
    }
}