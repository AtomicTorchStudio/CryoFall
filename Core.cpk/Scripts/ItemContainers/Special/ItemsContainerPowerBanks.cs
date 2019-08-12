namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerPowerBanks : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            return context.Item.ProtoItem is IProtoItemPowerBank
                   || (context.Item.ProtoItem is IProtoItemWithFuel protoItemWithFuel
                       && protoItemWithFuel.ItemFuelConfig.IsElectricity);
        }
    }
}