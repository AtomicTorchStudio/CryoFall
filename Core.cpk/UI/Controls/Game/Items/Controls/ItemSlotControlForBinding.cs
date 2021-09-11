namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ItemSlotControlForBinding : BaseControl
    {
        public static readonly DependencyProperty IsBackgroundEnabledProperty =
            DependencyProperty.Register(nameof(IsBackgroundEnabled),
                                        typeof(bool),
                                        typeof(ItemSlotControlForBinding),
                                        new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsSelectableProperty =
            DependencyProperty.Register(
                nameof(IsSelectable),
                typeof(bool),
                typeof(ItemSlotControlForBinding),
                new PropertyMetadata(true, IsSelectablePropertyChanged));

        public static readonly DependencyProperty ContainerProperty =
            DependencyProperty.Register(
                nameof(Container),
                typeof(IClientItemsContainer),
                typeof(ItemSlotControlForBinding),
                new PropertyMetadata(null, ContainerPropertyChanged));

        public static readonly DependencyProperty SlotIdProperty =
            DependencyProperty.Register(
                nameof(SlotId),
                typeof(int),
                typeof(ItemSlotControlForBinding),
                new PropertyMetadata(0, SlotIdPropertyChanged));

        private IClientItemsContainer cachedContainer;

        private byte cachedSlotId;

        private bool isSubscribedOnContainerEvents;

        private ItemSlotControl itemSlotControl;

        static ItemSlotControlForBinding()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ItemSlotControlForBinding),
                new FrameworkPropertyMetadata(typeof(ItemSlotControlForBinding)));
        }

        public IClientItemsContainer Container
        {
            get => (IClientItemsContainer)this.GetValue(ContainerProperty);
            set => this.SetValue(ContainerProperty, value);
        }

        public bool IsBackgroundEnabled
        {
            get => (bool)this.GetValue(IsBackgroundEnabledProperty);
            set => this.SetValue(IsBackgroundEnabledProperty, value);
        }

        public bool IsSelectable
        {
            get => (bool)this.GetValue(IsSelectableProperty);
            set => this.SetValue(IsSelectableProperty, value);
        }

        public int SlotId
        {
            get => (int)this.GetValue(SlotIdProperty);
            set => this.SetValue(SlotIdProperty, value);
        }

        protected override void InitControl()
        {
            this.itemSlotControl = (ItemSlotControl)VisualTreeHelper.GetChild(this, 0);
            this.Setup();
        }

        protected override void OnLoaded()
        {
            this.Setup();
        }

        protected override void OnUnloaded()
        {
            if (this.cachedContainer is null)
            {
                return;
            }

            this.UnsubscribeContainerEvents();
            this.cachedContainer = null;
        }

        private static void ContainerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ItemSlotControlForBinding)d).Setup();
        }

        private static void IsSelectablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ItemSlotControlForBinding)d).Setup();
        }

        private static void SlotIdPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ItemSlotControlForBinding)d).Setup();
        }

        private void ContainerItemsItemAddedHandler(IItem item)
        {
            if (item.ContainerSlotId == this.cachedSlotId)
            {
                this.RefreshSlot();
            }
        }

        private void ContainerItemsItemRemovedHandler(IItem item, byte slotId)
        {
            if (slotId == this.cachedSlotId)
            {
                this.RefreshSlot();
            }
        }

        private void ContainerItemsResetHandler()
        {
            this.RefreshSlot();
        }

        private void RefreshSlot()
        {
            this.itemSlotControl.RefreshItem();
        }

        private void Setup()
        {
            if (!this.isLoaded)
            {
                return;
            }

            var currentContainer = this.Container;
            if (this.cachedContainer == currentContainer)
            {
                return;
            }

            if (this.cachedContainer != currentContainer)
            {
                if (this.cachedContainer is not null)
                {
                    this.UnsubscribeContainerEvents();
                }

                this.cachedContainer = currentContainer;

                if (this.cachedContainer is not null)
                {
                    this.SubscribeContainerEvents();
                }
            }

            this.cachedSlotId = (byte)this.SlotId;

            if (this.itemSlotControl is null)
            {
                return;
            }

            this.itemSlotControl.IsSelectable = this.IsSelectable;
            this.itemSlotControl.Setup(this.cachedContainer, this.cachedSlotId);
        }

        private void SubscribeContainerEvents()
        {
            if (!this.isLoaded
                || this.isSubscribedOnContainerEvents
                || this.cachedContainer is null)
            {
                return;
            }

            this.isSubscribedOnContainerEvents = true;
            this.cachedContainer.ItemAdded += this.ContainerItemsItemAddedHandler;
            this.cachedContainer.ItemRemoved += this.ContainerItemsItemRemovedHandler;
            this.cachedContainer.ItemsReset += this.ContainerItemsResetHandler;
        }

        private void UnsubscribeContainerEvents()
        {
            if (!this.isSubscribedOnContainerEvents)
            {
                return;
            }

            this.isSubscribedOnContainerEvents = false;
            this.cachedContainer.ItemAdded -= this.ContainerItemsItemAddedHandler;
            this.cachedContainer.ItemRemoved -= this.ContainerItemsItemRemovedHandler;
            this.cachedContainer.ItemsReset -= this.ContainerItemsResetHandler;
        }
    }
}