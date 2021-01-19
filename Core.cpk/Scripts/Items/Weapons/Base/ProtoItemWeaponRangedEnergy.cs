namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterEnergySystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public abstract class ProtoItemWeaponRangedEnergy
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemWeaponRanged
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemWeaponEnergy
        where TPrivateState : WeaponPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public sealed override ushort AmmoCapacity => 0;

        public sealed override double AmmoReloadDuration => 0;

        public abstract double EnergyUsePerShot { get; }

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsEnergy>();

        public override bool SharedCanFire(ICharacter character, WeaponState weaponState)
        {
            var requiredEnergyAmount = SkillWeaponsEnergy.SharedGetRequiredEnergyAmount(character,
                this.EnergyUsePerShot);
            if (CharacterEnergySystem.SharedHasEnergyCharge(character, requiredEnergyAmount))
            {
                return true;
            }

            if (IsClient && weaponState.SharedGetInputIsFiring())
            {
                CharacterEnergySystem.ClientShowNotificationNotEnoughEnergyCharge(this);
                // stop using weapon item!
                weaponState.ProtoWeapon.ClientItemUseFinish(weaponState.ItemWeapon);
            }

            return false;
        }

        // Please note: the check SharedCanFire() has been already passed
        public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
        {
            if (IsClient)
            {
                // on client we cannot consume energy
                return true;
            }

            var requiredEnergyAmount = SkillWeaponsEnergy.SharedGetRequiredEnergyAmount(character,
                this.EnergyUsePerShot);
            return CharacterEnergySystem.ServerDeductEnergyCharge(character, requiredEnergyAmount);
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.UsesPowerBanks);
        }

        protected sealed override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = null;
            this.PrepareProtoWeaponRangedEnergy(ref overrideDamageDescription);
        }

        protected abstract void PrepareProtoWeaponRangedEnergy(ref DamageDescription damageDescription);

        protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.RangedEnergy;
        }
    }

    public abstract class ProtoItemWeaponRangedEnergy
        : ProtoItemWeaponRangedEnergy
            <WeaponPrivateState, EmptyPublicState, EmptyClientState>
    {
    }
}