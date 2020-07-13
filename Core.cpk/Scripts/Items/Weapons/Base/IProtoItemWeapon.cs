namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Damage;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public interface IProtoItemWeapon
        : IProtoItemWithCharacterAppearance,
          IProtoItemWithDurability,
          IProtoItemWithHotbarOverlay
    {
        ushort AmmoCapacity { get; }

        ushort AmmoConsumptionPerShot { get; }

        /// <summary>
        /// Reload duration in seconds.
        /// </summary>
        double AmmoReloadDuration { get; }

        bool CanDamageStructures { get; }

        string CharacterAnimationAimingName { get; }

        CollisionGroup CollisionGroup { get; }

        IReadOnlyList<IProtoItemAmmo> CompatibleAmmoProtos { get; }

        /// <summary>
        /// Damage apply delay in seconds.
        /// Damage will be raycasted and calculated after this delay when player starting the attack.
        /// Can be 0.
        /// </summary>
        double DamageApplyDelay { get; }

        double DamageMultiplier { get; }

        DamageStatsComparisonPreset DamageStatsComparisonPreset { get; }

        /// <summary>
        /// Duration of firing animation in seconds.
        /// Must be equal or smaller than <see cref="FireInterval" />.
        /// </summary>
        double FireAnimationDuration { get; }

        /// <summary>
        /// Fire interval in seconds.
        /// </summary>
        double FireInterval { get; }

        /// <summary>
        /// Time until current fire sequence/pattern is reset.
        /// </summary>
        double FirePatternCooldownDuration { get; }

        WeaponFirePatternPreset FirePatternPreset { get; }

        WeaponFireScatterPreset FireScatterPreset { get; }

        WeaponFireTracePreset FireTracePreset { get; }

        bool IsLoopedAttackAnimation { get; }

        DamageDescription OverrideDamageDescription { get; }

        double RangeMultiplier { get; }

        /// <summary>
        /// Delay (in seconds) when selecting this weapon in hotbar.
        /// </summary>
        double ReadyDelayDuration { get; }

        IProtoItemAmmo ReferenceAmmoProto { get; }

        /// <summary>
        /// Sound preset defining hit sounds upon various materials.
        /// </summary>
        ReadOnlySoundPreset<ObjectMaterial> SoundPresetHit { get; }

        ReadOnlySoundPreset<WeaponSound> SoundPresetWeapon { get; }

        (float min, float max) SoundPresetWeaponDistance { get; }

        (float min, float max) SoundPresetWeaponDistance3DSpread { get; }

        double SpecialEffectProbability { get; }

        ProtoSkillWeapons WeaponSkillProto { get; }

        ITextureResource WeaponTextureResource { get; }

        float ShotVolumeMultiplier { get; }

        void ClientOnFireModChanged(bool isFiring, uint shotsDone);

        void ClientOnWeaponHitOrTrace(
            ICharacter firingCharacter,
            Vector2D worldPositionSource,
            IProtoItemWeapon protoWeapon,
            IProtoItemAmmo protoAmmo,
            IProtoCharacter protoCharacter,
            in Vector2Ushort fallbackCharacterPosition,
            IReadOnlyList<WeaponHitData> hitObjects,
            in Vector2D endPosition,
            bool endsWithHit);

        void ClientOnWeaponShot(ICharacter character);

        void ClientPlayWeaponHitSound(
            [CanBeNull] IWorldObject hitWorldObject,
            IProtoWorldObject protoWorldObject,
            WeaponFireScatterPreset fireScatterPreset,
            ObjectMaterial objectMaterial,
            Vector2D worldObjectPosition);

        string GetCharacterAnimationNameFire(ICharacter character);

        void ServerOnShot(
            ICharacter character,
            IItem weaponItem,
            IProtoItemWeapon protoWeapon,
            IReadOnlyList<IWorldObject> hitObjects);

        bool SharedCanFire(ICharacter character, WeaponState weaponState);

        bool SharedOnFire(ICharacter character, WeaponState weaponState);

        void SharedOnHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            out bool isDamageStop);

        void SharedOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition);

        void SharedOnWeaponAmmoChanged(IItem item, ushort ammoCount);

        double SharedUpdateAndGetFirePatternCurrentSpreadAngleDeg(WeaponState weaponState);
    }
}