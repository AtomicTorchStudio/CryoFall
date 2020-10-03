namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ItemSlotControl : BaseControl, IItemSlotControl
    {
        public static readonly DependencyProperty IsBackgroundEnabledProperty =
            DependencyProperty.Register(nameof(IsBackgroundEnabled),
                                        typeof(bool),
                                        typeof(ItemSlotControl),
                                        new PropertyMetadata(defaultValue: true));

        private Border border;

        private IItemsContainer container;

        private string currentPrimaryStateName;

        private string currentSecondaryStateName;

        private bool isSelectable = true;

        private IItem item;

        private ItemSlotControlEventsHelper itemSlotControlEventsHelper;

        private FrameworkElement layoutRoot;

        private FrameworkElement tooltip;

        static ItemSlotControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ItemSlotControl),
                new FrameworkPropertyMetadata(typeof(ItemSlotControl)));
        }

        public ItemSlotControl()
        {
        }

        public IItemsContainer Container
        {
            get => this.container;
            private set
            {
                //// sometimes it's required in order to reset the slot
                //// so we will NOT do the commented out check below
                //if (this.container == value)
                //{
                //    return;
                //}

                this.container = value;
                this.RefreshItem();
            }
        }

        public bool IsBackgroundEnabled
        {
            get => (bool)this.GetValue(IsBackgroundEnabledProperty);
            set => this.SetValue(IsBackgroundEnabledProperty, value);
        }

        public bool IsSelectable
        {
            get => this.isSelectable;
            set
            {
                if (this.isSelectable == value)
                {
                    return;
                }

                this.isSelectable = value;

                if (this.isLoaded)
                {
                    this.RefreshActivityState();
                }
            }
        }

        public IItem Item
        {
            get => this.item;
            private set
            {
                if (this.Item == value)
                {
                    return;
                }

                if (value is not null
                    && !this.isLoaded)
                {
                    return;
                }

                this.item = value;
                var currentViewModel = this.DataContext as BaseViewModel;
                if (currentViewModel is not null)
                {
                    this.DataContext = null;
                    currentViewModel.Dispose();
                }

                this.RefreshDataContext();
                this.RefreshTooltip();
            }
        }

        public byte SlotId { get; private set; }

        public void RefreshHighlight(IItem selectedItem)
        {
            string stateName;

            if (selectedItem is not null
                && this.container is not null)
            {
                var canAddItem = Api.Client.Items.CanPlaceItem(
                    selectedItem,
                    this.container,
                    allowSwapping: true,
                    slotId: this.SlotId);

                // TODO: there is also "PlaceAllowedNormal" to highlight slot as green. Maybe we need to use it in some cases?
                stateName = canAddItem ? "Default" : "PlaceDisallowed";
            }
            else
            {
                stateName = "Default";
            }

            this.SetCurrentPrimaryState(stateName, true);
        }

        public void RefreshItem()
        {
            this.Item = this.isLoaded
                            ? this.container?.GetItemAtSlot(this.SlotId)
                            : null;
        }

        public void ResetControlForCache()
        {
            this.Container = null;

            if (this.isLoaded)
            {
                this.ResetStates();
            }
        }

        public void Setup(IItemsContainer setContainer, byte slotId)
        {
            this.SlotId = slotId;
            this.Container = setContainer;
        }

        public void SetupCustomMouseClickHandler(ItemSlotControlEventsHelper.ItemSlotMouseClickDelegate handler)
        {
            this.itemSlotControlEventsHelper.CustomMouseClickHandler = handler;
        }

        protected override void InitControl()
        {
            this.itemSlotControlEventsHelper = new ItemSlotControlEventsHelper(this);
            this.DataContext = null; // explicitly set null data context
        }

        protected override void OnLoaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            base.OnLoaded();

            // this cannot be done in InitControl, as later control might be removed and re-added
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            this.border = templateRoot.GetByName<Border>("Border");
            this.layoutRoot = templateRoot.GetByName<Grid>("LayoutRoot");

            this.RefreshItem();
            this.ResetStates();
            this.RefreshActivityState();
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            base.OnUnloaded();
            this.RefreshActivityState();

            this.Container = null;
        }

        private void DestroyTooltip()
        {
            if (this.tooltip is null)
            {
                return;
            }

            ToolTipServiceExtend.SetToolTip(this.layoutRoot, null);
            this.tooltip = null;
        }

        private void RefreshActivityState()
        {
            if (!this.IsSelectable)
            {
                ClientItemsManager.UnregisterSlotControl(this);
                this.DestroyTooltip();
                return;
            }

            if (this.isLoaded)
            {
                ClientItemsManager.RegisterSlotControl(this);
                this.RefreshTooltip();
            }
            else
            {
                ClientItemsManager.UnregisterSlotControl(this);
                this.DestroyTooltip();
            }

            if (this.isLoaded)
            {
                this.SetCurrentPrimaryState("Default", false);
            }
        }

        private void RefreshDataContext()
        {
            var viewModel = this.DataContext as ViewModelItem;
            if (viewModel is not null)
            {
                if (viewModel.Item == this.item)
                {
                    return;
                }

                this.DataContext = null;
                viewModel.Dispose();
            }

            this.DataContext = this.item is not null
                                   ? new ViewModelItem(this.item)
                                   : null;
        }

        private void RefreshTooltip()
        {
            this.DestroyTooltip();

            if (!this.IsMouseOver
                || this.item is null)
            {
                // no need to display the tooltip
                return;
            }

            this.tooltip = ItemTooltipControl.Create(this.item);
            ToolTipServiceExtend.SetToolTip(this.layoutRoot, this.tooltip);
        }

        private void ResetStates()
        {
            this.SetCurrentPrimaryState("Default", false);
            this.SetCurrentSecondaryState("Normal", false);
        }

        private void SetCurrentPrimaryState(string stateName, bool withTransition)
        {
            if (this.currentPrimaryStateName == stateName)
            {
                return;
            }

            VisualStateManager.GoToElementState(this.border, stateName, withTransition);
            this.currentPrimaryStateName = stateName;
        }

        private void SetCurrentSecondaryState(string stateName, bool withTransition)
        {
            if (this.currentSecondaryStateName == stateName)
            {
                return;
            }

            VisualStateManager.GoToElementState(this.border, stateName, withTransition);
            this.currentSecondaryStateName = stateName;
        }

        public class ItemSlotControlEventsHelper
        {
            private const double MouseUpEventMinDurationSinceDown = 0.15;

            private static double lastMouseDownTime;

            public ItemSlotMouseClickDelegate CustomMouseClickHandler;

            private readonly ItemSlotControl itemSlotControl;

            public ItemSlotControlEventsHelper(ItemSlotControl itemSlotControl)
            {
                this.itemSlotControl = itemSlotControl;
                itemSlotControl.MouseLeftButtonDown += this.SlotMouseButtonDownHandler;
                itemSlotControl.MouseRightButtonDown += this.SlotMouseButtonDownHandler;
                itemSlotControl.MouseLeftButtonUp += this.SlotMouseButtonUpHandler;
                itemSlotControl.MouseRightButtonUp += this.SlotMouseButtonUpHandler;
                itemSlotControl.MouseEnter += this.SlotMouseEnterHandler;
                itemSlotControl.MouseLeave += this.SlotMouseLeaveHandler;
            }

            public delegate bool ItemSlotMouseClickDelegate(bool isDown);

            private void SlotMouseButtonDownHandler(object sender, MouseButtonEventArgs e)
            {
                if (!this.itemSlotControl.IsSelectable)
                {
                    return;
                }

                e.Handled = true;

                if (this.CustomMouseClickHandler?.Invoke(isDown: true) ?? false)
                {
                    return;
                }

                // remember that mouse was pressed on this control
                lastMouseDownTime = Api.Client.Core.ClientRealTime;

                var isLeftMouseButton = e.ChangedButton == MouseButton.Left;
                if (ClientItemsManager.OnSlotSelected(this.itemSlotControl, isLeftMouseButton, isDown: true))
                {
                    this.itemSlotControl.DestroyTooltip();
                }
            }

            private void SlotMouseButtonUpHandler(object sender, MouseButtonEventArgs e)
            {
                if (!this.itemSlotControl.IsSelectable)
                {
                    return;
                }

                e.Handled = true;

                if (this.CustomMouseClickHandler?.Invoke(isDown: false) ?? false)
                {
                    return;
                }

                if (Api.Client.Core.ClientRealTime - lastMouseDownTime
                    < MouseUpEventMinDurationSinceDown)
                {
                    // don't act in case the mouse button was released too quickly
                    return;
                }

                if (ClientItemsManager.ItemInHand is not null
                    && e.ChangedButton == MouseButton.Left)
                {
                    // try to drop item here
                    if (ClientItemsManager.OnSlotSelected(this.itemSlotControl,
                                                          isLeftMouseButton: true,
                                                          isDown: false))
                    {
                        this.itemSlotControl.DestroyTooltip();
                    }
                }
            }

            private void SlotMouseEnterHandler(object sender, MouseEventArgs e)
            {
                this.itemSlotControl.RefreshTooltip();

                if (this.itemSlotControl.IsSelectable)
                {
                    this.itemSlotControl.SetCurrentSecondaryState("MouseOver", true);
                }
            }

            private void SlotMouseLeaveHandler(object sender, MouseEventArgs e)
            {
                this.itemSlotControl.DestroyTooltip();

                if (this.itemSlotControl.IsSelectable)
                {
                    this.itemSlotControl.SetCurrentSecondaryState("Normal", true);
                }
            }
        }
    }
}