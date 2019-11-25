namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IProtoVehicleAssemblyBay : IProtoObjectStructure, IInteractableProtoWorldObject
    {
        Vector2D PlatformCenterWorldOffset { get; }

        void SharedGetVehiclesOnPlatform(
            IStaticWorldObject vehicleAssemblyBay,
            ITempList<IDynamicWorldObject> result);

        bool SharedIsBaySpaceBlocked(IStaticWorldObject vehicleAssemblyBay);
    }
}