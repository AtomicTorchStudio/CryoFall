namespace AtomicTorch.CBND.CoreMod.ClientComponents.AmbientSound
{
    using AtomicTorch.CBND.CoreMod.ClientOptions.Audio;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ComponentAmbientSoundEmitter : ClientComponent
    {
        private const double VolumeInterpolationRate = 20;

        private double currentInterpolatedVolume;

        private IComponentSoundEmitter soundEmitter;

        private double targetVolume;

        public ComponentAmbientSoundEmitter()
            : base(isLateUpdateEnabled: true)
        {
        }

        public double CurrentInterpolatedVolume => this.currentInterpolatedVolume;

        public override void LateUpdate(double deltaTime)
        {
            this.currentInterpolatedVolume = MathHelper.LerpWithDeltaTime(
                this.currentInterpolatedVolume,
                this.targetVolume,
                Api.Client.Core.DeltaTime,
                rate: VolumeInterpolationRate);

            this.soundEmitter.Volume = (float)this.currentInterpolatedVolume;
        }

        /// <param name="volumeceCoef">Distance coef (in [0;1] range, validate before call)</param>
        /// <param name="suppression">Distance coef (in [0;1] range, validate before call)</param>
        public void SetTargetVolume(double volume)
        {
            volume *= AudioOptionVolumeAmbient.CurrentVolumeForAmbientSounds;
            this.targetVolume = volume;
        }

        public void Setup(SoundResource soundResource)
        {
            this.soundEmitter.Stop();
            this.soundEmitter.SoundResource = soundResource;
            this.soundEmitter.IsLooped = true;
            this.soundEmitter.IsUseSoundVolumeOption = false; // we control ambient volume separately from other sounds
            this.soundEmitter.Volume = (float)(this.currentInterpolatedVolume = this.targetVolume = 0);
            this.soundEmitter.Seek(
                Client.Core.ClientRealTime); // the ambient sound should never start from the beginning
            this.soundEmitter.Play();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.soundEmitter.Destroy();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.soundEmitter = Client.Audio.CreateSoundEmitter(this.SceneObject,
                                                                SoundResource.NoSound,
                                                                is3D: false,
                                                                isLooped: false,
                                                                volume: 0,
                                                                isPlaying: false);
        }
    }
}