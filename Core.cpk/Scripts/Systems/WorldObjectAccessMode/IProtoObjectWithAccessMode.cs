namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectWithAccessMode : IProtoWorldObject
    {
        bool CanChangeFactionRoleAccessForSelfRole { get; }

        bool IsClosedAccessModeAvailable { get; }

        bool IsEveryoneAccessModeAvailable { get; }
    }
}