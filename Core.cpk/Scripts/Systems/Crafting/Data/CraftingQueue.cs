namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;

    public class CraftingQueue : BaseNetObject
    {
        [NonSerialized]
        private IItemsContainer[] inputContainersArray;

        /// <summary>
        /// Create crafting queue. Store it in server state of the object and update with Update() method.
        /// </summary>
        /// <param name="containerInput">Container with ingredients.</param>
        /// <param name="containerOutput">Container where the crafted items will go.</param>
        public CraftingQueue(
            [NotNull] IItemsContainer containerInput,
            [NotNull] IItemsContainer containerOutput)
        {
            this.QueueItems = new NetworkSyncList<CraftingQueueItem>();
            this.ContainerInput = containerInput;
            this.ContainerOutput = containerOutput;
        }

        protected CraftingQueue()
        {
            this.QueueItems = new NetworkSyncList<CraftingQueueItem>();
        }

        [SyncToClient]
        public virtual IItemsContainer ContainerInput { get; }

        [SyncToClient]
        public virtual IItemsContainer ContainerOutput { get; }

        [TempOnly]
        public ushort? ContainerOutputLastStateHash { get; set; }

        public IItemsContainer[] InputContainersArray 
            => this.inputContainersArray ??= this.CreateInputContainersArray();

        /// <summary>
        /// Special flag meaning that output container cannot accommodate output items of current recipe.
        /// </summary>
        [SyncToClient(DeliveryMode.ReliableSequenced, maxUpdatesPerSecond: 2)]
        [TempOnly]
        public bool IsContainerOutputFull { get; set; }

        [SyncToClient]
        public NetworkSyncList<CraftingQueueItem> QueueItems { get; }

        public ushort ServerLastQueueItemLocalId { get; set; }

        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            networkDataType: typeof(float),
            maxUpdatesPerSecond: ScriptingConstants.NetworkDefaultMaxUpdatesPerSecond)]
        public double TimeRemainsToComplete { get; set; }

        public void Clear()
        {
            this.QueueItems.Clear();
        }

        public void SetDurationFromCurrentRecipe()
        {
            var craftingQueueItem = this.QueueItems.FirstOrDefault();
            if (craftingQueueItem == null)
            {
                this.TimeRemainsToComplete = double.MaxValue;
                return;
            }

            var recipe = craftingQueueItem.Recipe;
            if (this.GameObject is ICharacter character)
            {
                this.TimeRemainsToComplete = recipe.SharedGetDurationForPlayer(character);
                return;
            }

            this.TimeRemainsToComplete = recipe.OriginalDuration;
        }

        protected virtual IItemsContainer[] CreateInputContainersArray()
        {
            return new[] { this.ContainerInput };
        }
    }
}