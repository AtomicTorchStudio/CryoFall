namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemOrganic : IProtoItem
    {
        ushort OrganicValue { get; }
    }
}