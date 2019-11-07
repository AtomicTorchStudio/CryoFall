namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public interface ICharacterPublicState : IPublicState
    {
        AppliedCharacterInput AppliedInput { get; }

        IProtoItemWeapon SelectedItemWeaponProto { get; }

        NetworkSyncList<IProtoStatusEffect> CurrentPublicStatusEffects { get; }

        CharacterCurrentStats CurrentStats { get; set; }

        bool IsDead { get; set; }

        // Item selected in the hotbar or vehicle.
        IItem SelectedItem { get; }

        void ServerEnsureEverythingCreated();

        void SharedSetCurrentWeaponProtoOnly(IProtoItemWeapon weaponProto);

        void SharedSetSelectedItem(IItem item);
    }
}