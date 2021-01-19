namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons.Mech
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ComponentSkeletonMechAimingSoundManager : ClientComponent
    {
        private const float MaxVolume = 0.2f;

        private ICharacterPublicState characterPublicState;

        private double? cooldownRemains;

        private float lastAngleRad;

        private IComponentSkeleton skeleton;

        private IComponentSoundEmitter soundEmitter;

        public ComponentSkeletonMechAimingSoundManager()
            : base(isLateUpdateEnabled: true)
        {
        }

        public override void LateUpdate(double deltaTime)
        {
            if (!this.skeleton.IsEnabled)
            {
                this.Destroy();
                return;
            }

            if (this.characterPublicState.SelectedItemWeaponProto is null)
            {
                this.soundEmitter.Volume = 0;
                return;
            }

            var currentAngleRad = this.GetCurrentAngleRad();

            var deltaAngleRad = this.lastAngleRad - currentAngleRad;
            float targetVolume;
            if (Math.Abs(deltaAngleRad) >= 2 * MathConstants.DegToRad)
            {
                targetVolume = MaxVolume;
                this.lastAngleRad = currentAngleRad;
            }
            else
            {
                // the volume change is not large enough
                targetVolume = 0;
            }

            if (targetVolume <= 0)
            {
                if (this.cooldownRemains.HasValue)
                {
                    this.cooldownRemains -= deltaTime;
                    if (this.cooldownRemains.Value <= 0)
                    {
                        this.cooldownRemains = null;
                    }

                    targetVolume = MaxVolume;
                }
            }
            else if (targetVolume > 0)
            {
                if (!this.cooldownRemains.HasValue)
                {
                    // started playing - enable the cooldown so the sound cannot stop playing until the cooldown reached
                    this.cooldownRemains = 0.25;
                }
                else if (this.cooldownRemains.Value > 0)
                {
                    this.cooldownRemains -= deltaTime;
                    const double minCooldown = 0.1;
                    if (this.cooldownRemains < minCooldown)
                    {
                        this.cooldownRemains = minCooldown;
                    }
                }
            }

            this.soundEmitter.Volume = (float)MathHelper.LerpWithDeltaTime(this.soundEmitter.Volume,
                                                                           targetVolume,
                                                                           deltaTime,
                                                                           rate: 80);
        }

        public void Setup(
            IComponentSkeleton skeleton,
            ICharacter characterRotationSource,
            SoundResource soundResourceAimingProcess)
        {
            this.skeleton = skeleton;
            this.characterPublicState = characterRotationSource.GetPublicState<ICharacterPublicState>();

            this.soundEmitter = Api.Client.Audio.CreateSoundEmitter(this.SceneObject,
                                                                    soundResourceAimingProcess,
                                                                    is3D: !characterRotationSource
                                                                              .IsCurrentClientCharacter,
                                                                    isLooped: true,
                                                                    isPlaying: true,
                                                                    volume: 0);
            this.lastAngleRad = this.GetCurrentAngleRad();
        }

        protected override void OnDisable()
        {
            this.soundEmitter.Destroy();
            this.soundEmitter = null;
        }

        private float GetCurrentAngleRad()
        {
            return this.characterPublicState.AppliedInput.RotationAngleRad;
        }
    }
}