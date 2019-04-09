namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectSign : IProtoObjectStructure
    {
        void ClientSetSignText(IStaticWorldObject worldObjectSign, string signText);
    }
}