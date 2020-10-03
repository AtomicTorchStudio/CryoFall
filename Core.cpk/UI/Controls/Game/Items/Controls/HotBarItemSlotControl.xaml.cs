namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HotbarItemSlotControl : BaseUserControl, IItemSlotControl
    {
        public static readonly DependencyProperty IsShortcutAvailableProperty =
            DependencyProperty.Register("IsShortcutAvailable",
                                        typeof(bool),
                                        typeof(HotbarItemSlotControl),
                                        new PropertyMetadata(defaultValue: true));

        private bool isActive;

        private ItemSlotControl itemSlotControl;

        private ViewModelHotbarItemSlotControl viewModel;

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;
                this.viewModel.SelectedVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsShortcutAvailable
        {
            get => (bool)this.GetValue(IsShortcutAvailableProperty);
            set => this.SetValue(IsShortcutAvailableProperty, value);
        }

        public void RefreshItem()
        {
            this.itemSlotControl.RefreshItem();
            this.viewModel.Item = this.itemSlotControl.Item;
        }

        public void ResetControlForCache()
        {
            this.itemSlotControl.ResetControlForCache();
        }

        public void Setup(IItemsContainer setContainer, byte slotId)
        {
            this.itemSlotControl.Setup(setContainer, slotId);
            this.itemSlotControl.SetupCustomMouseClickHandler(this.CustomItemSlotControlMouseClickHandler);
            this.viewModel.Item = setContainer.GetItemAtSlot(slotId);
            this.viewModel.ShortcutKey = GetShortcutKey(slotId);
            this.IsActive = slotId == ClientHotbarSelectedItemManager.SelectedSlotId;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void InitControl()
        {
            this.itemSlotControl = this.GetByName<ItemSlotControl>("ItemSlotControl");
            this.DataContext = this.viewModel = new ViewModelHotbarItemSlotControl();
        }

        protected override void OnLoaded()
        {
            ClientHotbarSelectedItemManager.SelectedSlotIdChanged += this.SelectedHotbarSlotIdChangedHandler;
        }

        protected override void OnUnloaded()
        {
            ClientHotbarSelectedItemManager.SelectedSlotIdChanged -= this.SelectedHotbarSlotIdChangedHandler;
            this.viewModel.Item = null;
        }

        private static string GetShortcutKey(int slotId)
        {
            slotId++;
            if (slotId == 10)
            {
                slotId = 0;
            }

            return slotId.ToString();
        }

        private bool CustomItemSlotControlMouseClickHandler(bool isDown)
        {
            if (WindowsManager.OpenedWindowsCount > 0
                || ClientItemsManager.ItemInHand is not null
                || Api.Client.Input.IsKeyHeld(InputKey.Alt, evenIfHandled: true))
            {
                // allow working with item slot control as usual
                return false;
            }

            if (!isDown)
            {
                // no action on mouse button up over the hotbar
                return true;
            }

            // no menus opened, no use-click - select this hotbar slot instead
            ClientHotbarSelectedItemManager.SelectedSlotId = this.itemSlotControl.SlotId;
            return true;
        }

        private void SelectedHotbarSlotIdChangedHandler(byte? slotId)
        {
            this.IsActive = slotId == this.itemSlotControl.SlotId;
        }
    }
}