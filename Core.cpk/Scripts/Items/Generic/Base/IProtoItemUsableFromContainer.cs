namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemUsableFromContainer : IProtoItem
    {
        string ItemUseCaption { get; }
    }
}