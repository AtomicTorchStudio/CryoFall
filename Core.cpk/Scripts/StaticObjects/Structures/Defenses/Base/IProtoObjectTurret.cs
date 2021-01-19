namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectTurret : IProtoObjectStructure
    {
        void ClientSetTurretMode(IStaticWorldObject objectTurret, TurretMode mode);
    }
}