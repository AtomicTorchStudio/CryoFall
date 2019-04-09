namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectBarrel : IProtoObjectStructure
    {
        ushort LiquidCapacity { get; }

        ProtoBarrelPrivateState GetBarrelPrivateState(IStaticWorldObject objectManufacturer);
    }
}