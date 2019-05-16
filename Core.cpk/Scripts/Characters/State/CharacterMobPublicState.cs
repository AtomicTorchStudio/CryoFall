namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using static GameApi.Data.State.SyncToClientReceivers;

    public class CharacterMobPublicState : BasePublicState, ICharacterPublicState
    {
        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public AppliedCharacterInput AppliedInput { get; private set; }

        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public IProtoItemWeapon CurrentItemWeaponProto { get; private set; }

        [SyncToClient]
        [TempOnly]
        public CharacterCurrentStats CurrentStats { get; set; }

        [SyncToClient]
        [TempOnly]
        public bool IsDead { get; set; }

        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public IItem SelectedHotbarItem { get; private set; }

        public void EnsureEverythingCreated()
        {
            this.AppliedInput = new AppliedCharacterInput();

            if (this.CurrentStats == null)
            {
                // create current stats
                this.CurrentStats = new CharacterCurrentStats();
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