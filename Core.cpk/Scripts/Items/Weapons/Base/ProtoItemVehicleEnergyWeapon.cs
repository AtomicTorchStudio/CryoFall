namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public abstract class ProtoItemVehicleEnergyWeapon : ProtoItemVehicleWeaponRanged
    {
        public sealed override ushort AmmoCapacity => 0;

        public sealed override double AmmoReloadDuration => 0;

        // need to be installed on a mech in order to use it
        public override bool CanBeSelectedInVehicle => false;

        public override string CharacterAnimationAimingName => "WeaponAiming1Hand";

        public override string CharacterAnimationAimingRecoilName => "WeaponShooting1Hand";

        public abstract ushort EnergyUsePerShot { get; }

        public override bool SharedCanFire(ICharacter character, WeaponState weaponState)
        {
            var requiredEnergyAmount = this.EnergyUsePerShot;
            var vehicle = character.SharedGetCurrentVehicle();
            if (VehicleEnergySystem.SharedHasEnergyCharge(vehicle, requiredEnergyAmount))
            {
                return true;
            }

            if (IsClient && weaponState.SharedGetInputIsFiring())
            {
                VehicleEnergyConsumptionSystem.ClientShowNotificationNotEnoughEnergy(
                    (IProtoVehicle)vehicle.ProtoGameObject);
                // stop using weapon item!
                weaponState.ProtoWeapon.ClientItemUseFinish(weaponState.ItemWeapon);
            }

            return false;
        }

        public override bool SharedCanSelect(IItem item, ICharacter character, bool isAlreadySelected, bool isByPlayer)
        {
            if (IsClient
                && !isAlreadySelected
                && isByPlayer)
            {
                NotificationSystem.ClientShowNotification(
                    this.Name,
                    CoreStrings.Vehicle_Mech_NotificationWeaponNeedsInstallationOnMech,
                    NotificationColor.Bad,
                    item.ProtoItem.Icon);
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

            var requiredEnergyAmount = this.EnergyUsePerShot;
            var vehicle = character.SharedGetCurrentVehicle();
            return VehicleEnergySystem.ServerDeductEnergyCharge(vehicle, requiredEnergyAmount);
        }

        protected sealed override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = null;
            this.PrepareProtoWeaponRangedEnergy(ref overrideDamageDescription);
        }

        protected abstract void PrepareProtoWeaponRangedEnergy(ref DamageDescription damageDescription);
    }
}