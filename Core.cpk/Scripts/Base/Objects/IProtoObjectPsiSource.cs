namespace AtomicTorch.CBND.CoreMod.Objects
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectPsiSource : IProtoWorldObject
    {
        double PsiIntensity { get; }

        double PsiRadiusMax { get; }

        double PsiRadiusMin { get; }

        bool ServerIsPsiSourceActive(IWorldObject worldObject);
    }
}