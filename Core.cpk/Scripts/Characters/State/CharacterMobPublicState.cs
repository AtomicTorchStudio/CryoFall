namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using static GameApi.Data.State.SyncToClientReceivers;

    public class CharacterMobPublicState : BasePublicState, ICharacterPublicState
    {
        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public AppliedCharacterInput AppliedInput { get; private set; }

        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public IProtoItemWeapon SelectedItemWeaponProto { get; private set; }

        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public NetworkSyncList<IProtoStatusEffect> CurrentPublicStatusEffects { get; private set; }

        [SyncToClient]
        [TempOnly] // yes, mobs stats (currently only the health points) is not saved as HP is regenerated anyway
        public CharacterCurrentStats CurrentStats { get; set; }

        [SyncToClient]
        [TempOnly]
        public bool IsDead { get; set; }

        [SyncToClient(receivers: ScopePlayers)]
        [TempOnly]
        public IItem SelectedItem { get; private set; }

        public void ServerEnsureEverythingCreated()
        {
            this.AppliedInput = new AppliedCharacterInput();
            this.CurrentPublicStatusEffects = new NetworkSyncList<IProtoStatusEffect>();

            if (this.CurrentStats == null)
            {
                // create current stats
                this.CurrentStats = new CharacterCurrentStats();
            }
        }

        public void SharedSetCurrentWeaponProtoOnly(IProtoItemWeapon weaponProto)
        {
            this.SelectedItem = null;
            this.SelectedItemWeaponProto = weaponProto;
        }

        public void SharedSetSelectedItem(IItem item)
        {
            this.SelectedItem = item;
            this.SelectedItemWeaponProto = item?.ProtoItem as IProtoItemWeapon;
        }
    }
}