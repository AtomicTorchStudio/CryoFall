namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public delegate CreateItemResult DelegateSpawnDropItem(IProtoItem protoItem, ushort count);
}