namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    /// <summary>
    /// Item prototype for item with durability.
    /// </summary>
    public abstract class ProtoItemWithDurability
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItem
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemWithDurability
        where TPrivateState : ItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public abstract uint DurabilityMax { get; }

        public abstract bool IsRepairable { get; }

        public sealed override ushort MaxItemsPerStack => 1;

        public virtual void ClientCreateItemSlotOverlayControls(IItem item, List<Control> controls)
        {
            if (this.DurabilityMax > 0)
            {
                controls.Add(ItemSlotDurabilityOverlayControl.Create(item));
            }
        }

        public virtual void ServerOnItemBrokeAndDestroyed(IItem item, IItemsContainer container, byte slotId)
        {
        }

        public virtual void ServerOnItemDamaged(IItem item, double damageApplied)
        {
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            ItemDurabilitySystem.ServerInitializeItem(data.PrivateState, data.IsFirstTimeInit);
        }
    }

    /// <summary>
    /// Item prototype for item with durability.
    /// </summary>
    public abstract class ProtoItemWithDurability
        : ProtoItemWithDurability
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}