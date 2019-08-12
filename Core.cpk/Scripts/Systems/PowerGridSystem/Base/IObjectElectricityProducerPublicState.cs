namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public interface IObjectElectricityProducerPublicState : IObjectPublicStateWithActiveFlag
    {
        ElectricityProducerState ElectricityProducerState { get; set; }
    }
}