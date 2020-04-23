namespace AtomicTorch.CBND.CoreMod.Items
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.ItemFreshnessSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Helpers;

    public abstract class ProtoItemWithFreshness
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemGeneric
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemWithFreshness
        where TPrivateState : BasePrivateState, IItemWithFreshnessPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public abstract TimeSpan FreshnessDuration { get; }

        public uint FreshnessMaxValue { get; private set; }

        /// <summary>
        /// For all items with freshness the default value is "Tiny" stack size.
        /// </summary>
        public override ushort MaxItemsPerStack => ItemStackSize.Small;

        // Rare update rate is required otherwise there is not enough
        // floating point number precision to spoil the food accurately regarding the time.
        // It also reduces the server load as there are a lot of items with freshness (food, etc).
        public sealed override double ServerUpdateIntervalSeconds => 60;

        public void ClientCreateItemSlotOverlayControls(IItem item, List<Control> controls)
        {
            if (this.FreshnessMaxValue > 0)
            {
                controls.Add(ItemSlotFreshnessOverlayControl.Create(item));
            }
        }

        public override void ClientTooltipCreateControls(IItem item, List<Control> controls)
        {
            if (this.FreshnessMaxValue > 0)
            {
                controls.Add(ItemTooltipInfoFreshnessControl.Create(item));
            }

            base.ClientTooltipCreateControls(item, controls);
        }

        protected sealed override void PrepareProtoItem()
        {
            base.PrepareProtoItem();
            this.FreshnessMaxValue = ItemFreshnessSystem.SharedCalculateFreshnessMaxValue(this);
            this.PrepareProtoItemWithFreshness();
        }

        protected virtual void PrepareProtoItemWithFreshness()
        {
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            ItemFreshnessSystem.ServerInitializeItem(data.PrivateState, data.IsFirstTimeInit);
        }

        protected override void ServerOnStackItems(ServerOnStackItemData data)
        {
            base.ServerOnStackItems(data);

            // source item
            var item1 = data.ItemFrom;
            var item1State = GetPrivateState(item1);
            var item1Freshness = item1State.FreshnessCurrent;

            // destination item
            var item2 = data.ItemTo;
            var item2State = GetPrivateState(item2);
            var item2Freshness = item2State.FreshnessCurrent;

            var deltaCount = data.CountStacked;

            // calculate average freshness between the already existing (item2) food count
            // and the added food count (item1) and its freshness
            var previousItem2Count = item2.Count - deltaCount;
            var newItem2Freshness = ((ulong)item1Freshness * (ulong)deltaCount
                                     + (ulong)item2Freshness * (ulong)previousItem2Count)
                                    / item2.Count;

            item2State.FreshnessCurrent = (uint)MathHelper.Clamp(newItem2Freshness, 0, uint.MaxValue);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            ItemFreshnessSystem.ServerUpdateFreshness(data.GameObject, data.DeltaTime);
        }
    }

    /// <summary>
    /// Item prototype for food items (without state).
    /// </summary>
    public abstract class ProtoItemWithFreshness
        : ProtoItemWithFreshness
            <ItemWithFreshnessPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}