namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    // This item is no longer used after to introduction of fuel cells, but we're planning to restore it.
    public class ItemReactorCorePragmium : ProtoItemWithDurability
    {
        public const uint EnergyCapacity = 360_000;

        public override string Description =>
            "Pragmium-filled reactor core. Relatively stable, portable and long-lasting power source.";

        public override uint DurabilityMax => EnergyCapacity;

        public override bool IsRepairable => false;

        public override string Name => "Pragmium reactor core";

        public override void ServerOnItemBrokeAndDestroyed(IItem item, IItemsContainer container, byte slotId)
        {
            // place an empty reactor to the released container slot
            Server.Items.CreateItem<ItemReactorCoreEmpty>(container, slotId: slotId);
        }
    }
}