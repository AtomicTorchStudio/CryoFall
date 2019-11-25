namespace AtomicTorch.CBND.CoreMod.ClientComponents.AmbientSound
{
    using AtomicTorch.CBND.CoreMod.ClientOptions.Audio;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ComponentAmbientSoundEmitter : ClientComponent
    {
        private const double VolumeInterpolationRate = 20;

        private double currentInterpolatedVolume;

        private IComponentSoundEmitter soundEmitter;

        private double targetVolume;

        private static readonly IAudioClientService Audio = Api.Client.Audio;

        public ComponentAmbientSoundEmitter()
            : base(isLateUpdateEnabled: true)
        {
        }

        public double CurrentInterpolatedVolume => this.currentInterpolatedVolume;

        public bool IsUsingAmbientVolume { get; private set; }

        public override void LateUpdate(double deltaTime)
        {
            this.currentInterpolatedVolume = MathHelper.LerpWithDeltaTime(
                this.currentInterpolatedVolume,
                this.targetVolume,
                Api.Client.Core.DeltaTime,
                rate: VolumeInterpolationRate);

            this.soundEmitter.Volume = (float)this.currentInterpolatedVolume;
        }

        public void SetTargetVolume(double volume)
        {
            if (this.IsUsingAmbientVolume)
            {
                volume *= AudioOptionVolumeAmbient.CurrentVolumeForAmbientSounds;
            }
            else
            {
                volume *= Audio.VolumeSounds;
            }

            this.targetVolume = volume;
        }

        public void Setup(SoundResource soundResource, bool isUsingAmbientVolume)
        {
            this.IsUsingAmbientVolume = isUsingAmbientVolume;
            this.soundEmitter.Stop();
            this.soundEmitter.SoundResource = soundResource;
            this.soundEmitter.IsLooped = true;
            this.soundEmitter.IsUseSoundVolumeOption = false; // we control ambient volume separately from other sounds
            this.soundEmitter.Volume = (float)(this.currentInterpolatedVolume = this.targetVolume = 0);
            // the ambient sound should never start from the beginning
            this.soundEmitter.Seek(RandomHelper.NextDouble() * 100);
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