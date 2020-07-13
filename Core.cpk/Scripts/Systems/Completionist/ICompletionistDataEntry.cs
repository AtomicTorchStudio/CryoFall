namespace AtomicTorch.CBND.CoreMod.Systems.Completionist
{
    using AtomicTorch.CBND.GameApi.Data;

    public interface ICompletionistDataEntry
    {
        bool IsRewardClaimed { get; }

        IProtoEntity Prototype { get; }
    }
}