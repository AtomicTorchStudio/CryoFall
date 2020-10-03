namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using static GameApi.Data.State.SyncToClientReceivers;

    public class PlayerCharacterPublicState
        : BasePublicState, ICharacterPublicStateWithEquipment
    {
        static PlayerCharacterPublicState()
        {
            if (Api.IsServer)
            {
                Api.Server.Characters.PlayerOnlineStateChanged += ServerPlayerCharacterOnlineStateChanged;
            }
        }

        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public AppliedCharacterInput AppliedInput { get; private set; }

        [SyncToClient]
        public string ClanTag { get; set; }

        [SyncToClient]
        public IItemsContainer ContainerEquipment { get; private set; }

        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public BasePublicActionState CurrentPublicActionState { get; set; }

        [SyncToClient] // we can set receivers: ScopePlayers but some players find it useful to mod the game and display it over their character
        [TempOnly]
        public NetworkSyncList<IProtoStatusEffect> CurrentPublicStatusEffects { get; private set; }

        public CharacterCurrentStats CurrentStats
        {
            get => this.CurrentStatsExtended;
            set => throw new Exception(
                       "Cannot change current stats - use " + nameof(this.CurrentStatsExtended) + " instead");
        }

        [SyncToClient]
        public PlayerCharacterCurrentStats CurrentStatsExtended { get; set; }

        [SyncToClient]
        public IDynamicWorldObject CurrentVehicle { get; private set; }

        [SyncToClient]
        public CharacterHumanFaceStyle FaceStyle { get; set; }

        [SyncToClient]
        [TempOnly]
        public bool IsDead { get; set; }

        [SyncToClient]
        public bool IsHeadEquipmentHiddenForSelfAndPartyMembers { get; set; }

        [SyncToClient]
        public bool IsMale { get; set; }

        [SyncToClient]
        [TempOnly]
        public bool IsNewbie { get; set; }

        [SyncToClient]
        public bool IsOnline { get; set; }

        [SyncToClient]
        public bool IsPveDuelModeEnabled { get; set; }

        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public IItem SelectedItem { get; private set; }

        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public IProtoItemWeapon SelectedItemWeaponProto { get; private set; }

        /// <summary>
        /// When player is requesting unstuck, the special timer will be displayed over the players' character.
        /// To know when the unstuck request will be fulfilled, the clients using this property.
        /// </summary>
        [SyncToClient]
        [TempOnly]
        public double UnstuckExecutionTime { get; set; }

        public void ServerEnsureEverythingCreated()
        {
            this.AppliedInput = new AppliedCharacterInput();
            this.CurrentPublicStatusEffects = new NetworkSyncList<IProtoStatusEffect>();
            this.CurrentStatsExtended ??= new PlayerCharacterCurrentStats();
        }

        public void ServerInitState(ICharacter character)
        {
            // prepare equipment container
            // see EquipmentType enum
            const byte equipmentSlotsCount = 10;
            this.ContainerEquipment ??= Api.Server.Items.CreateContainer(
                character,
                Api.GetProtoEntity<ItemsContainerCharacterEquipment>(),
                slotsCount: equipmentSlotsCount);
        }

        public void ServerSetCurrentVehicle(IDynamicWorldObject vehicle)
        {
            var character = (ICharacter)this.GameObject;
            if (this.CurrentVehicle is not null)
            {
                Api.Server.World.ExitPrivateScope(character, this.CurrentVehicle);
            }

            this.CurrentVehicle = vehicle;
            if (vehicle is null)
            {
                return;
            }

            Api.Server.World.EnterPrivateScope(character, this.CurrentVehicle);
        }

        public void SharedSetCurrentWeaponProtoOnly(IProtoItemWeapon weaponProto)
        {
            this.SelectedItem = null;
            this.SelectedItemWeaponProto = weaponProto;
        }

        public void SharedSetSelectedItem(IItem item)
        {
            if (ReferenceEquals(this.SelectedItem, item))
            {
                return;
            }

            var character = (ICharacter)this.GameObject;
            if (Api.IsServer)
            {
                this.SelectedItem?.ProtoItem.ServerItemHotbarSelectionChanged(this.SelectedItem,
                                                                              character,
                                                                              isSelected: false);
            }

            this.SelectedItem = item;

            if (Api.IsServer)
            {
                this.SelectedItem?.ProtoItem.ServerItemHotbarSelectionChanged(this.SelectedItem,
                                                                              character,
                                                                              isSelected: true);
            }

            this.SelectedItemWeaponProto = this.SelectedItem?.ProtoItem as IProtoItemWeapon;
        }

        private static void ServerPlayerCharacterOnlineStateChanged(ICharacter playerCharacter, bool isOnline)
        {
            if (!isOnline)
            {
                return;
            }

            var currentVehicle = PlayerCharacter.GetPublicState(playerCharacter).CurrentVehicle;
            if (currentVehicle is null)
            {
                return;
            }

            Api.Server.World.EnterPrivateScope(playerCharacter, currentVehicle);
        }
    }
}