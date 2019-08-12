namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using static WeaponSound;

    public static class WeaponsSoundPresets
    {
        public static readonly ReadOnlySoundPreset<WeaponSound> SpecialUseSkeletonSound
            = new SoundPreset<WeaponSound>().ToReadOnly();

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponMelee
            = new SoundPreset<WeaponSound>()
                .Add(Shot, "Weapons/Melee/Shot");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponMeleeEnergyLaserRapier
            = new SoundPreset<WeaponSound>()
                .Add(Shot, "Weapons/MeleeEnergy/LaserRapier/Shot");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRanged
            = new SoundPreset<WeaponSound>()
              .Add(Reload, "Weapons/Ranged/Reload")
              .Add(Empty,  "Weapons/Ranged/Empty");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedBow
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotBow");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedLaser
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotLaser");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedMachinegun
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotMachinegun");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedMusket
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotMusket");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedFlintlockPistol
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotFlintlockPistol");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedPistol
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotPistol");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedPlasma
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotPlasma");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedRevolver
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotRevolver");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedLuger
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotLuger");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedShotgunDoublebarreled
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotShotgunDoublebarreled");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedShotgunMilitary
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotShotgunMilitary");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedMachinePistol
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotMachinePistol");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedSMG
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotSMG");

        public static readonly ReadOnlySoundPreset<WeaponSound> WeaponRangedSniperRifle
            = WeaponRanged.Clone()
                          .Replace(Shot, "Weapons/Ranged/ShotSniperRifle");
    }
}