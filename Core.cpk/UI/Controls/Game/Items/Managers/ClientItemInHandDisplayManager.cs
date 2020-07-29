namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public static class ClientItemInHandDisplayManager
    {
        private static IClientItemsContainer handContainer;

        private static ItemInHandDisplayControl itemInHandDisplayControl;

        public static IClientItemsContainer HandContainer
        {
            get => handContainer;
            set
            {
                if (handContainer != value)
                {
                    if (handContainer != null)
                    {
                        handContainer.ItemAdded -= HandContainerItemAddedHandler;
                        handContainer.ItemRemoved -= HandContainerItemRemovedHandler;
                        handContainer.ItemsReset -= HandContainerItemsResetHandler;
                    }

                    handContainer = value;

                    if (handContainer != null)
                    {
                        handContainer.ItemAdded += HandContainerItemAddedHandler;
                        handContainer.ItemRemoved += HandContainerItemRemovedHandler;
                        handContainer.ItemsReset += HandContainerItemsResetHandler;
                    }
                }

                if (handContainer != null
                    && handContainer.OccupiedSlotsCount > 0)
                {
                    HandContainerItemAddedHandler(null);
                }
            }
        }

        public static void Init(IClientItemsContainer handContainer)
        {
            itemInHandDisplayControl ??= new ItemInHandDisplayControl();
            HandContainer = handContainer;
        }

        private static void HandContainerItemAddedHandler(IItem item)
        {
            RefreshSlotControl();
        }

        private static void HandContainerItemRemovedHandler(IItem item, byte slotId)
        {
            RefreshSlotControl();
        }

        private static void HandContainerItemsResetHandler()
        {
            RefreshSlotControl();
        }

        private static void RefreshSlotControl()
        {
            var item = handContainer.Items.FirstOrDefault();
            if (item != null)
            {
                itemInHandDisplayControl.Show();
            }
            else
            {
                itemInHandDisplayControl.Hide();
            }

            ClientItemsManager.RefreshHighlight(item);
        }
    }
}