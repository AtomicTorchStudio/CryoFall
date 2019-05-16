namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectBarrel : IProtoObjectStructure
    {
        ushort LiquidCapacity { get; }

        void ClientDrainBarrel(IStaticWorldObject worldObject);

        ProtoBarrelPrivateState GetBarrelPrivateState(IStaticWorldObject objectManufacturer);
    }
}