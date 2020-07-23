namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using static WeaponSound;

    public static class WeaponsSoundPresets
    {
        public static readonly ReadOnlySoundPreset<WeaponSound> SpecialUseSkeletonSound
            = new SoundPreset<WeaponSound>();

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRanged
            = new SoundPreset<WeaponSound>()
              .Add(Reload,         "Weapons/Ranged/Reload")
              .Add(ReloadFinished, "Weapons/Ranged/ReloadFinished")
              .Add(Empty,          "Weapons/Ranged/Empty");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponGrenadeLauncher
            = WeaponRanged.Clone()
                          .Replace(Shot,           "Weapons/Ranged/ShotGrenadeLauncher")
                          .Replace(Reload,         "Weapons/Ranged/ReloadGrenadeLauncher")
                          .Replace(ReloadFinished, "Weapons/Ranged/ReloadGrenadeLauncherFinished");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponMelee
            = new SoundPreset<WeaponSound>()
                .Add(Shot, "Weapons/Melee/Shot");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponMeleeEnergyLaserRapier
            = new SoundPreset<WeaponSound>()
                .Add(Shot, "Weapons/MeleeEnergy/LaserRapier/Shot");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedBoltActionRifle
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotBoltActionRifle");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedBow
            = WeaponRanged.Clone()
                          .Replace(Shot,           "Weapons/Ranged/ShotBow")
                          .Replace(Reload,         "Weapons/Ranged/ReloadCrossbow")
                          .Replace(ReloadFinished, "Weapons/Ranged/ReloadCrossbowFinished");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedFlintlockPistol
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotFlintlockPistol");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedLaserPistol
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotLaserPistol");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedLaserRifle
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotLaserRifle");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedLightRifle
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotLightRifle");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedLuger
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotLuger");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedMachinegun
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotMachinegun");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedMachinePistol
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotMachinePistol");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedMagnum
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotMagnum");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedMusket
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotMusket");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedPistol
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotPistol");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedPlasmaPistol
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotPlasmaPistol");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedPlasmaRifle
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotPlasmaRifle");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedRevolver
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotRevolver");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedShotgunDoublebarreled
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotShotgunDoublebarreled");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedShotgunMilitary
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotShotgunMilitary");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedSMG
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotSMG");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedSniperRifle
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotSniperRifle");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponVehicleGeneric
            = WeaponRanged.Clone()
                          .Replace(Reload, "Weapons/Vehicle/Reload");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponVehicleAutocannonHeavy
            = WeaponVehicleGeneric.Clone()
                                  .Replace(Shot, "Weapons/Vehicle/ShotAutocannonHeavy");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponVehicleAutocannonLight
            = WeaponVehicleGeneric.Clone()
                                  .Replace(Shot, "Weapons/Vehicle/ShotAutocannonLight");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponVehicleEnergyCannon
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Vehicle/ShotEnergyCannon");
    }
}