namespace AtomicTorch.CBND.CoreMod.Objects
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectHeatSource : IProtoWorldObject
    {
        double HeatIntensity { get; }

        double HeatRadiusMax { get; }

        double HeatRadiusMin { get; }
    }
}