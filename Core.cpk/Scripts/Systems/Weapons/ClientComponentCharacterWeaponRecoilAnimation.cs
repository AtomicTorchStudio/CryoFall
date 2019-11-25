namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ClientComponentCharacterWeaponRecoilAnimation : ClientComponent
    {
        private ICharacter character;

        private double duration;

        private double power;

        private IComponentSkeleton skeletonRenderer;

        private double time;

        private IProtoItemWeaponRanged weaponProto;

        public void StartRecoil(
            ICharacter character,
            IProtoItemWeaponRanged weaponProto,
            IComponentSkeleton skeletonRenderer)
        {
            this.character = character;
            this.weaponProto = weaponProto;
            this.skeletonRenderer = skeletonRenderer;
            this.duration = weaponProto.CharacterAnimationAimingRecoilDuration;
            this.time = 0;

            // add recoil power
            this.power += this.RandomizeRecoilPower(
                this.weaponProto.CharacterAnimationAimingRecoilPower
                * this.weaponProto.CharacterAnimationAimingRecoilPowerAddCoef);

            if (this.power > this.weaponProto.CharacterAnimationAimingRecoilPower)
            {
                // clamp recoil power
                this.power = this.weaponProto.CharacterAnimationAimingRecoilPower;
            }

            if (!this.IsEnabled)
            {
                this.IsEnabled = true;
            }

            this.Update(0);
        }

        public override void Update(double deltaTime)
        {
            this.time += deltaTime;
            if (this.time > this.duration)
            {
                // animation completed
                this.RemoveAnimationAndDisable();
                return;
            }

            if (this.skeletonRenderer.GetCurrentAnimationName(AnimationTrackIndexes.ItemAiming) == null)
            {
                // no aiming animation is active
                this.RemoveAnimationAndDisable();
                return;
            }

            var characterClientState = this.character.GetClientState<BaseCharacterClientState>();
            this.skeletonRenderer.SetAnimationFrame(
                trackIndex: AnimationTrackIndexes.ItemAimingRecoil,
                animationName: this.weaponProto.CharacterAnimationAimingRecoilName,
                timePositionFraction: characterClientState.LastAimCoef);

            // calculate animation progress (from 0 to 1, 1 means full recoil animation will be rendered)
            var alpha = (this.duration - this.time) / this.duration;
            alpha = EasyOut(alpha);
            alpha *= this.power;

            this.skeletonRenderer.SetAlphaForTrack(
                trackIndex: AnimationTrackIndexes.ItemAimingRecoil,
                alpha: alpha);
        }

        // The animation will look too linear, so let's apply an easing function.
        // It will give the recoil animation a smooth look when the weapon
        // is positioned slowly (over time) into the default position (as in real life).
        private static double EasyOut(double t)
        {
            return Math.Pow(t, 1.5);
        }

        /// <summary>
        /// It's too unrealistic if the recoil power is the same for every shot.
        /// Let's decrease the recoil power randomly a little bit.
        /// </summary>
        private double RandomizeRecoilPower(double power)
        {
            const double maxDifferenceFraction = 0.1; // max difference is 10%, yes, same for all weapon kinds
            var maxDifferenceValue = power * maxDifferenceFraction;
            var difference = maxDifferenceValue * RandomHelper.NextDouble();
            return power - difference;
        }

        private void RemoveAnimationAndDisable()
        {
            // do not destroy - reuse
            //this.Destroy();
            this.skeletonRenderer.RemoveAnimationTrack(
                trackIndex: AnimationTrackIndexes.ItemAimingRecoil);
            this.IsEnabled = false;
            this.power = 0;
        }
    }
}