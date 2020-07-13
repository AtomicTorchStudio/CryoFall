namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectSprinkler : IProtoObjectStructure, IInteractableProtoWorldObject
    {
        uint ElectricityConsumptionPerWatering { get; }

        double WaterCapacity { get; }

        double WaterConsumptionPerWatering { get; }

        void ClientWaterNow(IStaticWorldObject worldObjectSprinkler);

        SprinklerWateringRequestResult ServerTryWaterNow(IStaticWorldObject worldObjectSprinkler);

        SprinklerWateringRequestResult SharedCanWaterNow(IStaticWorldObject worldObjectSprinkler);
    }
}