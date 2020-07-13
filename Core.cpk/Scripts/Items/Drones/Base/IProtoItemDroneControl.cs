namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    public interface IProtoItemDroneControl : IProtoItemWithDurability
    {
        byte MaxDronesToControl { get; }
    }
}