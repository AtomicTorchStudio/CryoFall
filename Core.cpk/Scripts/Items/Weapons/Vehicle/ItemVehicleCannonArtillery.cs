namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Vehicle
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemVehicleCannonArtillery : ProtoItemVehicleWeaponGrenadeLauncher
    {
        public override ushort AmmoCapacity => 10;

        public override double AmmoReloadDuration => 5;

        // need to be installed on a mech in order to use it
        public override bool CanBeSelectedInVehicle => false;

        public override string CharacterAnimationAimingName => "WeaponAiming1Hand";

        public override double CharacterAnimationAimingRecoilDuration => 0.45;

        public override string CharacterAnimationAimingRecoilName => "WeaponShooting1Hand";

        public override double CharacterAnimationAimingRecoilPower => 0.9;

        public override double CharacterAnimationAimingRecoilPowerAddCoef => 1;

        public override string Description =>
            "Special heavy artillery cannon that uses high-explosive shells, which are ideal against heavily armored targets.";

        public override uint DurabilityMax => 200;

        public override double FireInterval => 1.5; // very slow

        public override double RangeMultiplier => 1.3; // this way shells will have 13 tile radius, same as sniper rifle

        public override string Name => "Artillery cannon";

        public override double ReadyDelayDuration => 0;

        public override string WeaponAttachmentName => "TurretLeft";

        public override VehicleWeaponHardpoint WeaponHardpoint => VehicleWeaponHardpoint.Large;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillVehicles>();

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

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.Artillery)
                       .Set(textureScreenOffset: (10, 7));
        }

        protected override void PrepareProtoGrenadeLauncher(out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoCannonShell>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponGrenadeLauncher;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}