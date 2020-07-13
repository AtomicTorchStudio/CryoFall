namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ItemDronePrivateState : ItemWithDurabilityPrivateState
    {
        public IDynamicWorldObject WorldObjectDrone { get; set; }
    }
}