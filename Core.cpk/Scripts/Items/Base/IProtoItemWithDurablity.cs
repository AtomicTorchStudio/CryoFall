namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemWithDurablity : IProtoItemWithSlotOverlay
    {
        ushort DurabilityMax { get; }

        void ServerOnItemBrokeAndDestroyed(IItem item, IItemsContainer container, byte slotId);

        /// <summary>
        /// This method is called when an item is damaged while character has it equipped.
        /// </summary>
        void ServerOnItemDamaged(IItem item, double damageApplied);
    }
}