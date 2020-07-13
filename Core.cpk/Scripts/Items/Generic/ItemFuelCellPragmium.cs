namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemFuelCellPragmium : ProtoItemWithDurability, IProtoItemFuelCell
    {
        public const uint EnergyCapacity = 360_000;

        public override string Description =>
            "Fuel cell filled with liquid pragmium. Lasts an extremely long time.";

        public override uint DurabilityMax => EnergyCapacity;

        public override bool IsRepairable => false;

        public override string Name => "Pragmium fuel cell";

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