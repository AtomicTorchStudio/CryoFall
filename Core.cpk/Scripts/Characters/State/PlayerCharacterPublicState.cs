namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using static GameApi.Data.State.SyncToClientReceivers;

    public class PlayerCharacterPublicState
        : BasePublicState, ICharacterPublicStateWithEquipment
    {
        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public AppliedCharacterInput AppliedInput { get; private set; }

        [SyncToClient]
        public IItemsContainer ContainerEquipment { get; private set; }

        [SyncToClient(receivers: ScopePlayers)]
        public IProtoItemWeapon CurrentItemWeaponProto { get; private set; }

        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public BasePublicActionState CurrentPublicActionState { get; set; }

        public CharacterCurrentStats CurrentStats
        {
            get => this.CurrentStatsExtended;
            set => throw new Exception(
                       "Cannot change current stats - use " + nameof(this.CurrentStatsExtended) + " instead");
        }

        [SyncToClient]
        public PlayerCharacterCurrentStats CurrentStatsExtended { get; set; }

        [SyncToClient]
        public CharacterHumanFaceStyle FaceStyle { get; set; }

        [SyncToClient]
        [TempOnly]
        public bool IsDead { get; set; }

        [SyncToClient]
        public bool IsMale { get; set; }

        [SyncToClient]
        public bool IsOnline { get; set; }

        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public IItem SelectedHotbarItem { get; private set; }

        public void EnsureEverythingCreated()
        {
            this.AppliedInput = new AppliedCharacterInput();

            if (this.CurrentStatsExtended == null)
            {
                this.CurrentStatsExtended = new PlayerCharacterCurrentStats();
            }
        }

        public void ServerInitState(ICharacter character)
        {
            // prepare equipment container
            // see EquipmentType enum
            const byte equipmentSlotsCount = 10;

            if (this.ContainerEquipment == null)
            {
                this.ContainerEquipment = Api.Server.Items.CreateContainer(
                    character,
                    Api.GetProtoEntity<ItemsContainerCharacterEquipment>(),
                    slotsCount: equipmentSlotsCount);
            }
        }

        public void SetCurrentWeaponProtoOnly(IProtoItemWeapon weaponProto)
        {
            this.SelectedHotbarItem = null;
            this.CurrentItemWeaponProto = weaponProto;
        }

        public void SetSelectedHotbarItem(IItem item)
        {
            this.SelectedHotbarItem = item;
            this.CurrentItemWeaponProto = item?.ProtoItem as IProtoItemWeapon;
        }
    }
}