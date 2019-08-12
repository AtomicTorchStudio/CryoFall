namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class RechargingStationItemsControl : BaseUserControl
    {
        public static readonly DependencyProperty ItemsContainerProperty =
            DependencyProperty.Register(nameof(ItemsContainer),
                                        typeof(IItemsContainer),
                                        typeof(RechargingStationItemsControl),
                                        new PropertyMetadata(default(IItemsContainer), PropertyChangedCallback));

        private GenericItemsContainerController<RechargingStationItemSlot> controller;

        private UIElementCollection stackPanelSlotsChildren;

        public IItemsContainer ItemsContainer
        {
            get => (IItemsContainer)this.GetValue(ItemsContainerProperty);
            set => this.SetValue(ItemsContainerProperty, value);
        }

        protected override void InitControl()
        {
            this.stackPanelSlotsChildren = this.GetByName<StackPanel>("StackPanelItemsSlots").Children;
            this.stackPanelSlotsChildren.Clear();

            if (IsDesignTime)
            {
                // dummy slots controls
                for (var i = 0; i < 4; i++)
                {
                    this.stackPanelSlotsChildren.Add(new RechargingStationItemSlot());
                }
            }
            else
            {
                this.controller = new GenericItemsContainerController<RechargingStationItemSlot>(
                    this.stackPanelSlotsChildren);
            }
        }

        protected override void OnLoaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.RefreshContainer();
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.controller.IsLoaded = false;
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RechargingStationItemsControl)d).RefreshContainer();
        }

        private void RefreshContainer()
        {
            if (!this.isLoaded)
            {
                return;
            }

            this.controller.SetContainer((IClientItemsContainer)this.ItemsContainer);
            this.controller.IsLoaded = true;
        }
    }
}