namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemsContainerControl : BaseUserControl
    {
        public static readonly DependencyProperty ContainerProperty
            = DependencyProperty.Register(
                nameof(Container),
                typeof(IClientItemsContainer),
                typeof(ItemsContainerControl),
                new PropertyMetadata(null, ContainerPropertyChanged));

        // Using a DependencyProperty as the backing store for DesignTimeSlotsCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DesignTimeSlotsCountProperty
            = DependencyProperty.Register(
                nameof(DesignTimeSlotsCount),
                typeof(int),
                typeof(ItemsContainerControl),
                new PropertyMetadata(4, DesignTimeSlotsCountPropertyChanged));

        private GenericItemsContainerController<ItemSlotControl> controller;

        private UIElementCollection wrapPanelItemsSlotsChildren;

        public IClientItemsContainer Container
        {
            get => (IClientItemsContainer)this.GetValue(ContainerProperty);
            set => this.SetValue(ContainerProperty, value);
        }

        public int DesignTimeSlotsCount
        {
            get => (int)this.GetValue(DesignTimeSlotsCountProperty);
            set => this.SetValue(DesignTimeSlotsCountProperty, value);
        }

        protected override void InitControl()
        {
            var wrapPanelItemsSlots = this.GetByName<WrapPanel>("WrapPanelItemsSlots");
            this.wrapPanelItemsSlotsChildren = wrapPanelItemsSlots.Children;

            if (IsDesignTime)
            {
                this.CreateDummySlots();
            }
            else
            {
                this.controller =
                    new GenericItemsContainerController<ItemSlotControl>(this.wrapPanelItemsSlotsChildren);
                this.RefreshContainer();
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            if (IsDesignTime)
            {
                return;
            }

            this.controller.IsLoaded = true;
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            if (IsDesignTime)
            {
                return;
            }

            this.controller.IsLoaded = false;
        }

        private static void ContainerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ItemsContainerControl)d).RefreshContainer();
        }

        private static void DesignTimeSlotsCountPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (IsDesignTime)
            {
                var itemsContainerControl = (ItemsContainerControl)d;
                itemsContainerControl?.CreateDummySlots();
            }
        }

        private void CreateDummySlots()
        {
            if (!IsDesignTime)
            {
                throw new Exception("Cannot create dummy slots in non-DesignTime");
            }

            if (this.wrapPanelItemsSlotsChildren == null)
            {
                return;
            }

            this.wrapPanelItemsSlotsChildren.Clear();

            // dummy slots controls
            for (var i = 0; i < this.DesignTimeSlotsCount; i++)
            {
                this.wrapPanelItemsSlotsChildren.Add(new ItemSlotControl());
            }
        }

        private void RefreshContainer()
        {
            this.controller?.SetContainer(this.Container);
        }
    }
}