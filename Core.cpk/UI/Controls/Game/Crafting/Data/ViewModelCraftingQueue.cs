namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ViewModelCraftingQueue : BaseViewModel
    {
        public const string NotificationCannotDegradeableCraftItem_Message =
            "As this recipe involves degradable components, you cannot cancel it.";

        public const string NotificationCannotDegradeableCraftItem_Title = "Cannot cancel this recipe";

        private readonly CraftingQueue craftingQueue;

        private readonly Dictionary<CraftingQueueItem, CraftingQueueItemControl> craftingQueueItemsControls
            = new Dictionary<CraftingQueueItem, CraftingQueueItemControl>();

        private readonly UIElementCollection itemsChildrenCollection;

        private NetworkSyncList<CraftingQueueItem> craftingQueueItems;

        public ViewModelCraftingQueue(CraftingQueue craftingQueue, UIElementCollection itemsChildrenCollection)
        {
            this.craftingQueue = craftingQueue;
            this.itemsChildrenCollection = itemsChildrenCollection;

            this.craftingQueueItems = craftingQueue.QueueItems;

            this.craftingQueueItems.ClientElementInserted += this.QueueElementInsertedHandler;
            this.craftingQueueItems.ClientElementRemoved += this.QueueClientElementRemovedHandler;

            this.Refresh();

            foreach (var craftingQueueItem in this.craftingQueueItems)
            {
                this.AddControl(craftingQueueItem);
            }

            this.ItemControlHiddenHandler();
        }

        public Visibility CraftingQueueControlVisibility { get; set; } = Visibility.Visible;

        public Visibility ProgressVisibility { get; set; } = Visibility.Visible;

        protected override void DisposeViewModel()
        {
            this.craftingQueueItems.ClientElementInserted -= this.QueueElementInsertedHandler;
            this.craftingQueueItems.ClientElementRemoved -= this.QueueClientElementRemovedHandler;
            this.craftingQueueItems = null;

            base.DisposeViewModel();
        }

        private void AddControl(CraftingQueueItem craftingQueueItem)
        {
            var control = new CraftingQueueItemControl
            {
                DeleteCallback = this.DeleteCraftingQueueItemHandler,
                HiddenCallback = this.ItemControlHiddenHandler,
                CountToCraftChangedCallback = this.ItemControlCountToCraftChangedCallback
            };

            control.Setup(this.craftingQueue, craftingQueueItem);

            this.craftingQueueItemsControls[craftingQueueItem] = control;
            this.itemsChildrenCollection.Add(control);

            this.Refresh();
        }

        private void DeleteCraftingQueueItemHandler(CraftingQueueItem craftingQueueItem)
        {
            if (!craftingQueueItem.Recipe.IsCancellable)
            {
                NotificationSystem.ClientShowNotification(
                    NotificationCannotDegradeableCraftItem_Title,
                    NotificationCannotDegradeableCraftItem_Message,
                    icon: craftingQueueItem.Recipe.Icon);
                return;
            }

            CraftingSystem.Instance.ClientDeleteQueueItem(craftingQueueItem);
        }

        private void ItemControlCountToCraftChangedCallback(CraftingQueueItem _ = null)
        {
            this.Refresh();
        }

        private void ItemControlHiddenHandler(CraftingQueueItemControl _ = null)
        {
            if (this.itemsChildrenCollection.Count == 0)
            {
                this.CraftingQueueControlVisibility = Visibility.Collapsed;
            }
        }

        private void QueueClientElementRemovedHandler(
            NetworkSyncList<CraftingQueueItem> source,
            int index,
            CraftingQueueItem removedValue)
        {
            if (this.craftingQueueItems.Count == 0)
            {
                // queue finished sounds
                ClientComponentTimersManager.AddAction(
                    delaySeconds: 0.3,
                    action: () => Client.Audio.PlayOneShot(new SoundResource("UI/Crafting/Completed")));
            }

            this.RemoveControl(removedValue);
        }

        private void QueueElementInsertedHandler(
            NetworkSyncList<CraftingQueueItem> source,
            int index,
            CraftingQueueItem value)
        {
            this.AddControl(value);
        }

        private void Refresh()
        {
            var currentCraftingItem = this.craftingQueueItems.FirstOrDefault();
            this.ProgressVisibility = currentCraftingItem != null ? Visibility.Visible : Visibility.Collapsed;
            if (this.ProgressVisibility == Visibility.Visible)
            {
                this.CraftingQueueControlVisibility = Visibility.Visible;
            }
        }

        private void RemoveControl(CraftingQueueItem removedValue)
        {
            var control = this.craftingQueueItemsControls[removedValue];
            this.craftingQueueItemsControls.Remove(removedValue);
            control.Hide();
        }
    }
}