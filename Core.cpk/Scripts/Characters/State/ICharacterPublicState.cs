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

        IProtoItemWeapon CurrentItemWeaponProto { get; }

        CharacterCurrentStats CurrentStats { get; set; }

        bool IsDead { get; set; }

        NetworkSyncList<IProtoStatusEffect> CurrentPublicStatusEffects { get; }

        IItem SelectedHotbarItem { get; }

        void EnsureEverythingCreated();

        void SetCurrentWeaponProtoOnly(IProtoItemWeapon weaponProto);

        void SetSelectedHotbarItem(IItem item);
    }
}