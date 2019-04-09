namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectLight : IProtoObjectStructure
    {
        double FuelCapacity { get; }

        IFuelItemsContainer FuelItemsContainerPrototype { get; }

        void ClientSetServerMode(IStaticWorldObject lightObject, ObjectLightMode mode);
    }
}