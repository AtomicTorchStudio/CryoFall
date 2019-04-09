namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;

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
          IProtoItemWithDurablity
        where TPrivateState : BasePrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        protected ProtoItemTool()
        {
            this.Icon = new TextureResource("Items/Tools/" + this.GetType().Name);
        }

        public abstract ushort DurabilityMax { get; }

        public override double GroundIconScale => 1.5;

        public override ITextureResource Icon { get; }

        /// <summary>
        /// Tool cannot have stacks.
        /// </summary>
        public sealed override ushort MaxItemsPerStack => ItemStackSize.Single;

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