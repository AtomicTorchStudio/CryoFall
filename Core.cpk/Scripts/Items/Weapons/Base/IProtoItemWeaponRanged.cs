namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    public interface IProtoItemWeaponRanged : IProtoItemWeapon
    {
        double CharacterAnimationAimingRecoilDuration { get; }

        string CharacterAnimationAimingRecoilName { get; }

        /// <summary>
        /// Max recoil power (max distance of recoil animation).
        /// </summary>
        double CharacterAnimationAimingRecoilPower { get; }

        /// <summary>
        /// This coefficient defines how many of the recoil power will be added per shot.
        /// By default it's 1, so the full recoil power will be added on the first shot,
        /// but it can be overridden to any other value.
        /// This effect makes firing the automatic weapons much more fun.
        /// For example, if it's set to 1/3.0, then after 3 shots the full recoil power will be gained.
        /// </summary>
        double CharacterAnimationAimingRecoilPowerAddCoef { get; }

        IMuzzleFlashDescriptionReadOnly MuzzleFlashDescription { get; }
    }
}