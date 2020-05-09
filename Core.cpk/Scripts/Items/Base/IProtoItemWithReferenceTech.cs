namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Items;

    [NotPersistent]
    [NotNetworkAvailable]
    public interface IProtoItemWithReferenceTech : IProtoItem
    {
        TechNode ReferenceTech { get; }
    }
}