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
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemVehicleAutocannonLight : ProtoItemVehicleWeaponRanged
    {
        public override ushort AmmoCapacity => 100;

        public override double AmmoReloadDuration => 3;

        // need to be installed on a mech in order to use it
        public override bool CanBeSelectedInVehicle => false;

        public override string CharacterAnimationAimingName => "WeaponAiming1Hand";

        public override double CharacterAnimationAimingRecoilDuration => 0.45;

        public override string CharacterAnimationAimingRecoilName => "WeaponShooting1Hand";

        public override double CharacterAnimationAimingRecoilPower => 0.5;

        public override double CharacterAnimationAimingRecoilPowerAddCoef
            => 1 / 2.5; // full recoil power will be gained on third shot

        public override double DamageMultiplier => 0.8; // slightly lower than default

        public override string Description =>
            "Light autocannon designed for hand-mounted hardpoints on mechanized battle armor. Uses light-caliber ammo.";

        public override uint DurabilityMax => 800;

        public override double FireInterval => 1 / 10.0; // 10 per second

        public override string Name => "Light autocannon";

        public override double SpecialEffectProbability => 0.1;

        public override string WeaponAttachmentName => "TurretLeft";

        public override VehicleWeaponHardpoint WeaponHardpoint => VehicleWeaponHardpoint.Normal;

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

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new(
                initialSequence: new[] { 0.0, 0.5, 0.5 },
                cycledSequence: new[] { 1.5, 3.0, 2.5, 0.0, 3.5 });
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.ModernSubmachinegun)
                       .Set(textureScreenOffset: (39, 5), textureScale: 1.25);
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber10mm>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponVehicleAutocannonLight;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}