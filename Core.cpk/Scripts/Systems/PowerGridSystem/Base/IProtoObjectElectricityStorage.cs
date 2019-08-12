namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;

    public interface IProtoObjectElectricityStorage : IProtoObjectStructure, IInteractableProtoStaticWorldObject
    {
        double ElectricityCapacity { get; }
    }
}