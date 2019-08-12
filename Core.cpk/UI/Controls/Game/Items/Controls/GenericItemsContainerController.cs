namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;

    public class GenericItemsContainerController<TItemSlotControl>
        where TItemSlotControl : FrameworkElement, IItemSlotControl, new()
    {
        private static readonly TItemSlotControl[] EmptySlotsControls = new TItemSlotControl[0];

        private readonly UIElementCollection panelContainerChildren;

        private IClientItemsContainer container;

        private bool isLoaded;

        private bool isSubscribedOnContainerEvents;

        private TItemSlotControl[] slotsControls = EmptySlotsControls;

        public GenericItemsContainerController([NotNull] UIElementCollection panelContainerChildren)
        {
            this.panelContainerChildren = panelContainerChildren;
        }

        public delegate void SlotControlDelegate(TItemSlotControl itemSlotControl, byte slotId);

        public event SlotControlDelegate SlotControlAdded;

        public event SlotControlDelegate SlotControlRemoved;

        public bool IsLoaded
        {
            get => this.isLoaded;
            set
            {
                if (this.isLoaded == value)
                {
                    return;
                }

                this.isLoaded = value;

                if (this.isLoaded)
                {
                    this.RefreshItemSlots();
                    this.SubscribeContainerEvents();
                }
                else
                {
                    this.Cleanup();
                    this.UnsubscribeContainerEvents();
                }
            }
        }

        public UIElementCollection PanelContainerChildren => this.panelContainerChildren;

        public void SetContainer(IClientItemsContainer containerToSet)
        {
            if (this.container == containerToSet)
            {
                return;
            }

            this.UnsubscribeContainerEvents();
            this.container = containerToSet;
            this.RefreshItemSlots(true);
            this.SubscribeContainerEvents();
        }

        private void Cleanup()
        {
            if (this.slotsControls.Length == 0)
            {
                return;
            }

            this.PanelContainerChildren?.Clear();

            var itemSlotControlCache = ControlsCache<TItemSlotControl>.Instance;
            foreach (var releasedItemScontControl in this.slotsControls)
            {
                itemSlotControlCache.Push(releasedItemScontControl);
            }

            this.slotsControls = EmptySlotsControls;
        }

        private void ContainerItemsItemAddedHandler(IItem item)
        {
            this.slotsControls[item.ContainerSlotId].RefreshItem();
        }

        private void ContainerItemsItemRemovedHandler(IItem item, byte slotId)
        {
            this.slotsControls[slotId].RefreshItem();
        }

        private void ContainerItemsResetHandler()
        {
            this.RefreshItemSlots(forceResetSlots: true);
        }

        private void ContainerItemsSlotsCountChangedHandler()
        {
            this.RefreshItemSlots(forceResetSlots: false);
        }

        private void RefreshItemSlots(bool forceResetSlots = false)
        {
            if (!this.isLoaded)
            {
                return;
            }

            if (this.container == null)
            {
                //throw new Exception("Container is not assigned for " + this);
                return;
            }

            Api.Logger.Important("Refreshing items container: " + this.container);

            // Let's refresh items slots according to current container capacity:
            // 1) Copy required slots controls.
            // 2) Trim unused slots controls.
            // 3) Add new slots controls.
            var slotsCount = this.container.SlotsCount;

            // Use controls cache to reduce controls spawn/destroy rate.
            var controlsCache = ControlsCache<TItemSlotControl>.Instance;

            var previousSlotsControls = this.slotsControls;
            var previousSlotsCount = (byte)previousSlotsControls.Length;
            this.slotsControls = new TItemSlotControl[slotsCount];

            // 1) Copy required slots controls.
            if (previousSlotsCount > 0)
            {
                var copyCount = Math.Min(previousSlotsCount, slotsCount);
                for (var i = 0; i < copyCount; i++)
                {
                    this.slotsControls[i] = previousSlotsControls[i];
                }
            }

            // 2) Trim unused slots controls.
            for (var slotId = slotsCount; slotId < previousSlotsCount; slotId++)
            {
                var slotControl = previousSlotsControls[slotId];
                this.PanelContainerChildren.Remove(slotControl);
                controlsCache.Push(slotControl);
                this.SlotControlRemoved?.Invoke(slotControl, slotId);
                previousSlotsControls[slotId] = null;
            }

            // 3) Add new slots controls.
            for (var slotId = previousSlotsCount; slotId < slotsCount; slotId++)
            {
                var slotControl = controlsCache.Pop();
                this.PanelContainerChildren.Add(slotControl);
                slotControl.Setup(this.container, slotId);
                this.slotsControls[slotId] = slotControl;
                this.SlotControlAdded?.Invoke(slotControl, slotId);
                //itemSlotControl.IsActive = isActive;
            }

            if (!forceResetSlots)
            {
                return;
            }

            // If requested, reset all slots.
            // As optimization, we will reset only those slots, which were here previously.
            // New slots don't need reset because they were just set.
            // Please note that prevSlotsCount can be > slotsCount.
            previousSlotsCount = Math.Min(previousSlotsCount, slotsCount);
            for (byte slotId = 0; slotId < previousSlotsCount; slotId++)
            {
                var slotControl = this.slotsControls[slotId];
                slotControl.Setup(this.container, slotId);
            }
        }

        private void SubscribeContainerEvents()
        {
            if (!this.isLoaded
                || this.isSubscribedOnContainerEvents
                || this.container == null)
            {
                return;
            }

            this.isSubscribedOnContainerEvents = true;
            this.container.ItemAdded += this.ContainerItemsItemAddedHandler;
            this.container.ItemRemoved += this.ContainerItemsItemRemovedHandler;
            this.container.SlotsCountChanged += this.ContainerItemsSlotsCountChangedHandler;
            this.container.ItemsReset += this.ContainerItemsResetHandler;
        }

        private void UnsubscribeContainerEvents()
        {
            if (!this.isSubscribedOnContainerEvents)
            {
                return;
            }

            this.isSubscribedOnContainerEvents = false;
            this.container.ItemAdded -= this.ContainerItemsItemAddedHandler;
            this.container.ItemRemoved -= this.ContainerItemsItemRemovedHandler;
            this.container.SlotsCountChanged -= this.ContainerItemsSlotsCountChangedHandler;
            this.container.ItemsReset -= this.ContainerItemsResetHandler;
        }
    }
}