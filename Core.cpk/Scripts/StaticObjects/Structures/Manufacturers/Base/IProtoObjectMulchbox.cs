namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectMulchbox : IProtoObjectStructure
    {
        ushort OrganicCapacity { get; }

        ObjectMulchboxPrivateState GetMulchboxPrivateState(IStaticWorldObject objectMulchbox);
    }
}