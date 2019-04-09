namespace AtomicTorch.CBND.CoreMod.Systems.Resources
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectGatherable : IProtoStaticWorldObject
    {
        double DurationGatheringSeconds { get; }

        double GetGatheringSpeedMultiplier(IStaticWorldObject worldObject, ICharacter character);

        bool ServerGather(IStaticWorldObject worldObject, ICharacter character);

        bool SharedIsCanGather(IStaticWorldObject staticWorldObject);
    }
}