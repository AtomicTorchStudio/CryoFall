namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public interface IProtoItemAmmo : IProtoItem
    {
        DamageDescription DamageDescription { get; }

        bool IsSuppressWeaponSpecialEffect { get; }

        void ServerOnCharacterHit(ICharacter damagedCharacter, double damage);
    }
}