namespace AtomicTorch.CBND.CoreMod.ClientOptions.Audio
{
    using AtomicTorch.CBND.GameApi.Scripting;

    public sealed class AudioOptionVolumeAmbient : ProtoOptionSlider<AudioOptionsCategory>
    {
        private const double VolumeMultiplier = 0.5;

        private static double currentVolume;

        public AudioOptionVolumeAmbient()
        {
            this.ValueProvider = this.Default;
        }

        /// <summary>
        /// This property is used every time we need to set a volume for ambient sound emitters.
        /// </summary>
        public static double CurrentVolumeForAmbientSounds { get; private set; }

        public override double Default => Api.IsEditor ? 0 : 1;

        public override double Maximum => 1;

        public override double Minimum => 0;

        public override string Name => "Ambient volume";

        public override IProtoOption OrderAfterOption
            => GetProtoEntity<AudioOptionVolumeSounds>();

        public override double ValueProvider
        {
            get => currentVolume;
            set => currentVolume = value;
        }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            // TODO: ensure the ambient sound is playing so player can adjust the volume and hear the difference
            CurrentVolumeForAmbientSounds = this.CurrentValue * VolumeMultiplier;
        }
    }
}