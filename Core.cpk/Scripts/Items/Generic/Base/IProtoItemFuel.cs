namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemFuel : IProtoItem
    {
        double FuelAmount { get; }
    }
}