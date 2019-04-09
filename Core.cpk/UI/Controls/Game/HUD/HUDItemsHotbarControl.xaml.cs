namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDItemsHotbarControl : BaseUserControl
    {
        private GenericItemsContainerController<HotbarItemSlotControl> controller;

        private ControlTemplate controlTemplateSlotDelimiter;

        private UIElementCollection stackPanelSlotsChildren;

        private ViewModelHUDItemsHotbarControl viewModel;

        public HUDItemsHotbarControl()
        {
        }

        protected override void InitControl()
        {
            this.stackPanelSlotsChildren = this.GetByName<StackPanel>("StackPanelItemsSlots").Children;
            this.stackPanelSlotsChildren.Clear();

            if (IsDesignTime)
            {
                // dummy slots controls
                for (var i = 0; i < 10; i++)
                {
                    this.stackPanelSlotsChildren.Add(new HotbarItemSlotControl());
                }
            }
            else
            {
                this.controlTemplateSlotDelimiter =
                    Api.Client.UI.GetApplicationResource<ControlTemplate>(
                        "ItemSlotDelimiterAltVerticalControlTemplate");

                this.controller =
                    new GenericItemsContainerController<HotbarItemSlotControl>(this.stackPanelSlotsChildren);
                this.controller.SlotControlAdded += this.ControllerSlotControlAddedHandler;
                this.controller.SlotControlRemoved += this.ControllerSlotControlRemovedHandler;
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            if (IsDesignTime)
            {
                return;
            }

            var containerHotbar =
                (IClientItemsContainer)Api.Client.Characters.CurrentPlayerCharacter.SharedGetPlayerContainerHotbar();

            this.controller.SetContainer(containerHotbar);
            this.controller.IsLoaded = true;

            this.DataContext = this.viewModel = new ViewModelHUDItemsHotbarControl();
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            if (IsDesignTime)
            {
                return;
            }

            this.controller.IsLoaded = false;

            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private void ControllerSlotControlAddedHandler(HotbarItemSlotControl itemSlotControl, byte slotId)
        {
            if (slotId >= 9)
            {
                // last slot - do not add delimiter
                return;
            }

            var controlDelimiter = new ContentControl() { Template = this.controlTemplateSlotDelimiter };
            this.controller.PanelContainerChildren.Add(controlDelimiter);
            itemSlotControl.Tag = controlDelimiter;

            //if (slotId == 4)
            //{
            //	// increase spacing between first 5 slots and last 5 slots
            //	itemSlotControl.Margin = new Thickness(0, 0, 20, 0);
            //}
        }

        private void ControllerSlotControlRemovedHandler(HotbarItemSlotControl itemSlotControl, byte slotId)
        {
            if (itemSlotControl.Tag is ContentControl controlDelimiter)
            {
                this.controller.PanelContainerChildren.Remove(controlDelimiter);
            }

            itemSlotControl.Tag = null;
        }
    }
}