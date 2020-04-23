namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ItemPowerBankPrivateState : ItemWithDurabilityPrivateState
    {
        [SyncToClient(DeliveryMode.ReliableSequenced,
                      maxUpdatesPerSecond: 2,
                      networkDataType: typeof(float))]
        public double EnergyCharge { get; set; }
    }
}