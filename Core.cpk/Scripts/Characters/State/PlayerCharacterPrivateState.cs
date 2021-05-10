namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.CharacterOrigins;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Achievements;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDroneControl;
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class PlayerCharacterPrivateState : BaseCharacterPrivateState
    {
        private IStaticWorldObject currentBedObject;

        [SyncToClient]
        public PlayerCharacterAchievements Achievements { get; private set; }

        [SyncToClient]
        public PlayerCharacterCompletionistData CompletionistData { get; private set; }

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
        public ILogicObject DroneController { get; private set; }

        [SyncToClient]
        public NetworkSyncList<DroppedLootInfo> DroppedLootLocations { get; private set; }

        [TempOnly]
        public CharacterInput Input { get; set; }

        [SyncToClient]
        public bool IsAppearanceSelected { get; set; }

        [SyncToClient]
        public bool IsDespawned { get; set; }

        /// <summary>
        /// This is an AFK Mode flag which is activated when player is inactive for a while.
        /// </summary>
        [SyncToClient]
        [TempOnly]
        public bool IsIdle { get; set; }

        public Vector2Ushort LastDeathPosition { get; set; }

        public double? LastDeathTime { get; set; }

        [SyncToClient]
        public LastDismountedVehicleMapMark LastDismountedVehicleMapMark { get; set; }

        [SyncToClient]
        public double LastFactionLeaveTime { get; set; } = double.MinValue;

        [SyncToClient]
        public ProtoCharacterOrigin Origin { get; set; }

        [SyncToClient]
        [TempOnly]
        public NetworkSyncList<ILogicObject> OwnedLandClaimAreas { get; set; }

        // Store previously enabled selected hotbar slot Id in order to restore it when dismounting the vehicle.
        [TempOnly]
        [SyncToClient(isSendChanges: false)]
        public byte? PreviouslySelectedHotbarSlotId { get; set; }

        [SyncToClient]
        public PlayerCharacterQuests Quests { get; private set; }

        [SyncToClient(isSendChanges: false)]
        public byte? SelectedHotbarSlotId { get; set; }

        public double ServerFactionKarma { get; set; }

        [TempOnly]
        public byte ServerLastAckClientInputId { get; set; }

        public double ServerLastActiveTime { get; set; }

        [SyncToClient]
        public PlayerCharacterSkills Skills { get; private set; }

        [SyncToClient]
        public PlayerCharacterTechnologies Technologies { get; private set; }

        public void ServerInitState(ICharacter character)
        {
            this.DroppedLootLocations ??= new NetworkSyncList<DroppedLootInfo>();
            this.OwnedLandClaimAreas ??= new NetworkSyncList<ILogicObject>();

            var serverItemsService = Api.Server.Items;

            this.ContainerHand ??= serverItemsService.CreateContainer<ItemsContainerCharacterHand>(
                character,
                slotsCount: 1);

            this.ContainerInventory ??= serverItemsService.CreateContainer<ItemsContainerCharacterInventory>(
                character,
                slotsCount: PlayerConstants.InventorySlotsCount);

            this.ContainerHotbar ??= serverItemsService.CreateContainer<ItemsContainerCharacterHotbar>(
                character,
                slotsCount: PlayerConstants.HotbarSlotsCount);

            this.CraftingQueue ??= new CharacterCraftingQueue();
            this.Skills ??= new PlayerCharacterSkills();
            this.Technologies ??= new PlayerCharacterTechnologies();
            this.Quests ??= new PlayerCharacterQuests();
            this.Achievements ??= new PlayerCharacterAchievements();
            this.CompletionistData ??= new PlayerCharacterCompletionistData();
            this.DroneController ??= CharacterDroneControlSystem.ServerCreateCharacterDroneController();

            if (this.ServerLastActiveTime <= 0)
            {
                this.ServerLastActiveTime = Api.Server.Game.FrameTime;
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