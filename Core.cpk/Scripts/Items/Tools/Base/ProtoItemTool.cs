namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    /// <summary>
    /// Item prototype for tool.
    /// </summary>
    public abstract class ProtoItemTool
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItem
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemTool
        where TPrivateState : BasePrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        protected ProtoItemTool()
        {
            ToolsConstants.EnsureInitialized();
        }

        public abstract uint DurabilityMax { get; }

        public override double GroundIconScale => 1.25;

        public virtual bool IsRepairable => true;

        /// <summary>
        /// Tool cannot have stacks.
        /// </summary>
        public sealed override ushort MaxItemsPerStack => 1;

        public void ClientCreateItemSlotOverlayControls(IItem item, List<Control> controls)
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
            ItemDurabilitySystem.ServerModifyDurability(item, delta: -(int)damageApplied);
        }

        protected override string GenerateIconPath()
        {
            return "Items/Tools/" + this.GetType().Name;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            ItemDurabilitySystem.ServerInitializeItem(data.PrivateState, data.IsFirstTimeInit);
        }
    }

    /// <summary>
    /// Item prototype for tool.
    /// </summary>
    public abstract class ProtoItemTool
        : ProtoItemTool
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}