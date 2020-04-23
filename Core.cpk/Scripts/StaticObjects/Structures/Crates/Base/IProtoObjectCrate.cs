namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectCrate : IProtoObjectStructure
    {
        bool IsSupportItemIcon { get; }

        byte ItemsSlotsCount { get; }

        void ClientSetIconSource(IStaticWorldObject worldObjectCrate, IProtoEntity iconSource);
    }
}