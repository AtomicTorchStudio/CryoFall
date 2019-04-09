namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;

    public interface IProtoObjectWithAccessMode : IProtoObjectStructure
    {
        bool IsClosedAccessModeAvailable { get; }
    }
}