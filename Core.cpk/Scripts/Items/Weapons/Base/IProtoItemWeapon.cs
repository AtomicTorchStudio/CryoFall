namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoItemWeapon
        : IProtoItemWithCharacterAppearance,
          IProtoItemWithDurablity,
          IProtoItemWithHotbarOverlay
    {
        ushort AmmoCapacity { get; }

        ushort AmmoConsumptionPerShot { get; }

        /// <summary>
        /// Reload duration in seconds.
        /// </summary>
        double AmmoReloadDuration { get; }

        double ArmorPiercingMultiplier { get; }

        string CharacterAnimationAimingName { get; }

        IReadOnlyCollection<IProtoItemAmmo> CompatibleAmmoProtos { get; }

        /// <summary>
        /// Damage apply delay in seconds.
        /// Damage will be raycasted and calculated after this delay when player starting the attack.
        /// Can be 0.
        /// </summary>
        double DamageApplyDelay { get; }

        double DamageMultiplier { get; }

        /// <summary>
        /// Duration of firing animation in seconds.
        /// Must be equal or smaller than <see cref="FireInterval" />.
        /// </summary>
        double FireAnimationDuration { get; }

        /// <summary>
        /// Fire interval in seconds.
        /// </summary>
        double FireInterval { get; }

        bool IsLoopedAttackAnimation { get; }

        DamageDescription OverrideDamageDescription { get; }

        double RangeMultipier { get; }

        /// <summary>
        /// Sound preset defining hit sounds upon various materials.
        /// </summary>
        ReadOnlySoundPreset<ObjectSoundMaterial> SoundPresetHit { get; }

        ReadOnlySoundPreset<WeaponSound> SoundPresetWeapon { get; }

        (float min, float max) SoundPresetWeaponDistance { get; }

        ProtoSkillWeapons WeaponSkillProto { get; }

        ITextureResource WeaponTextureResource { get; }

        string GetCharacterAnimationNameFire(ICharacter character);

        void ServerOnDamageApplied(IItem weapon, ICharacter byCharacter, IWorldObject damagedObject, double damage);

        void ServerOnShot(
            ICharacter character,
            IItem weaponItem,
            IProtoItemWeapon protoWeapon,
            List<WeaponHitData> hitObjects);

        bool SharedCanFire(ICharacter character, WeaponState isFiringRequested);

        bool SharedOnFire(ICharacter character, WeaponState weaponState);
    }
}