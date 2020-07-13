namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemFuelCellGasoline : ProtoItemWithDurability, IProtoItemFuelCell
    {
        public const uint EnergyCapacity = 90_000;

        public override string Description =>
            "Fuel cell filled with standard liquid fuel. Cheap, but doesn't last very long.";

        public override uint DurabilityMax => EnergyCapacity;

        public override bool IsRepairable => false;

        public override string Name => "Gasoline fuel cell";

        public override void ServerOnItemBrokeAndDestroyed(IItem item, IItemsContainer container, byte slotId)
        {
            // place an empty fuel cell to the released container slot
            Server.Items.CreateItem<ItemFuelCellEmpty>(container, slotId: slotId);
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.FuelCellForVehicles);
        }
    }
}