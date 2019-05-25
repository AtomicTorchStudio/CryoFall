namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class PlayerCharacterPrivateState : BaseCharacterPrivateState
    {
        private IStaticWorldObject currentBedObject;

        [SyncToClient]
        public IItemsContainer ContainerHand { get; private set; }

        [SyncToClient]
        public IItemsContainer ContainerHotbar { get; private set; }

        [TempOnly]
        public ushort? ContainerHotbarLastStateHash { get; set; }

        [SyncToClient]
        public IItemsContainer ContainerInventory { get; private set; }

        [SyncToClient]
        public CraftingQueue CraftingQueue { get; private set; }

        [SubscribableProperty]
        [TempOnly]
        // Please note - this is not a SyncToClient property, it's processed simultaneously on the client and server.
        // Other clients doesn't receive this data. They receive snapshot of this data in the character public state.
        public IActionState CurrentActionState { get; private set; }

        /// <summary>
        /// Please note - this is not synchronized to the client.
        /// Only <see cref="CurrentBedObjectPosition" /> is synchronized.
        /// </summary>
        public IStaticWorldObject CurrentBedObject
        {
            get => this.currentBedObject;
            set
            {
                if (this.currentBedObject == value)
                {
                    return;
                }

                this.currentBedObject = value;
                this.CurrentBedObjectPosition = value?.TilePosition;
            }
        }

        [SyncToClient]
        public Vector2Ushort? CurrentBedObjectPosition { get; set; }

        [SyncToClient]
        public NetworkSyncList<DroppedLootInfo> DroppedLootLocations { get; private set; }

        [TempOnly]
        public CharacterInput Input { get; set; }

        [SyncToClient]
        public bool IsDespawned { get; set; }

        public Vector2Ushort LastDeathPosition { get; set; }

        // TODO: make non-temp in the future version
        [TempOnly]
        public double? LastDeathTime { get; set; }

        [SyncToClient]
        public PlayerCharacterQuests Quests { get; private set; }

        [SyncToClient(isSendChanges: false)]
        public byte? SelectedHotbarSlotId { get; set; }

        [TempOnly]
        public byte ServerLastAckClientInputId { get; set; }

        /// <summary>
        /// Used on PvE servers to despawn players who stay in offline for too long.
        /// </summary>
        // TODO: rename to ServerLastOnlineTime in A23
        public double? ServerOfflineSinceTime { get; set; }

        [SyncToClient]
        public PlayerCharacterSkills Skills { get; private set; }

        [SyncToClient]
        public PlayerCharacterTechnologies Technologies { get; private set; }

        public void ServerInitState(ICharacter character)
        {
            if (this.DroppedLootLocations == null)
            {
                this.DroppedLootLocations = new NetworkSyncList<DroppedLootInfo>();
            }

            var serverItemsService = Api.Server.Items;

            if (this.ContainerHand == null)
            {
                this.ContainerHand = serverItemsService.CreateContainer<ItemsContainerCharacterHand>(
                    character,
                    slotsCount: 1);
            }

            if (this.ContainerInventory == null)
            {
                this.ContainerInventory = serverItemsService.CreateContainer<ItemsContainerCharacterInventory>(
                    character,
                    slotsCount: 40);
            }

            if (this.ContainerHotbar == null)
            {
                this.ContainerHotbar = serverItemsService.CreateContainer<ItemsContainerCharacterHotbar>(
                    character,
                    slotsCount: 10);
            }

            if (this.CraftingQueue == null)
            {
                this.CraftingQueue = new CharacterCraftingQueue();
            }

            if (this.Skills == null)
            {
                this.Skills = new PlayerCharacterSkills();
            }

            if (this.Technologies == null)
            {
                this.Technologies = new PlayerCharacterTechnologies();
            }

            if (this.Quests == null)
            {
                this.Quests = new PlayerCharacterQuests();
            }
        }

        public void SetCurrentActionState(IActionState actionState)
        {
            this.CurrentActionState?.Cancel();
            this.CurrentActionState = actionState;
            actionState?.OnStart();
        }
    }
}