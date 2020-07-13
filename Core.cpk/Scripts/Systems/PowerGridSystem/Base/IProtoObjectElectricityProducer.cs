namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectElectricityProducer : IProtoObjectStructure
    {
        ElectricityThresholdsPreset DefaultGenerationElectricityThresholds { get; }

        int GenerationOrder { get; set; }

        IObjectElectricityStructurePrivateState GetPrivateState(IStaticWorldObject worldObject);

        IObjectElectricityProducerPublicState GetPublicState(IStaticWorldObject worldObject);

        void SharedGetElectricityProduction(
            IStaticWorldObject worldObject,
            out double currentProduction,
            out double maxProduction);
    }
}