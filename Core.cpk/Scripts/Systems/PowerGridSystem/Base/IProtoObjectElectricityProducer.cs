namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectElectricityProducer : IProtoObjectStructure
    {
        int GenerationOrder { get; set; }

        void SharedGetElectricityProduction(
            IStaticWorldObject worldObject,
            out double currentProduction,
            out double maxProduction);
    }
}