namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Damage;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IProtoItemAmmo : IProtoItem
    {
        IReadOnlyList<IProtoItemWeapon> CompatibleWeaponProtos { get; }

        DamageDescription DamageDescription { get; }

        DamageStatsComparisonPreset DamageStatsComparisonPreset { get; }

        WeaponFireTracePreset FireTracePreset { get; }

        bool IsReferenceAmmo { get; }

        bool IsSuppressWeaponSpecialEffect { get; }

        WeaponFireScatterPreset? OverrideFireScatterPreset { get; }

        void ClientOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition);

        void ClientOnObjectHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            WeaponHitData hitData,
            ref bool isDamageStop);

        void PrepareRegisterCompatibleWeapon(IProtoItemWeapon protoItemWeapon);

        void ServerOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition);

        void ServerOnObjectHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            ref bool isDamageStop);
    }
}