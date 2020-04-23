namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IProtoItemAmmo : IProtoItem
    {
        DamageDescription DamageDescription { get; }

        WeaponFireTracePreset FireTracePreset { get; }

        bool IsSuppressWeaponSpecialEffect { get; }

        WeaponFireScatterPreset? OverrideFireScatterPreset { get; }

        void ClientOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition);

        void ClientOnObjectHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            ref bool isDamageStop);

        void ServerOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition);

        void ServerOnObjectHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            ref bool isDamageStop);
    }
}