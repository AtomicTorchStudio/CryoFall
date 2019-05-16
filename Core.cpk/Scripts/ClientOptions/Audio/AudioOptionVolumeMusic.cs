namespace AtomicTorch.CBND.CoreMod.ClientOptions.Audio
{
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class AudioOptionVolumeMusic : ProtoOptionSlider<AudioOptionsCategory>
    {
        public override double Default => Api.IsEditor ? 0 : 1;

        public override double Maximum => 1;

        public override double Minimum => 0;

        public override string Name => "Music volume";

        public override IProtoOption OrderAfterOption
            => GetOption<AudioOptionVolumeSounds>();

        public override double ValueProvider
        {
            get => Client.Audio.VolumeMusic;
            set => Client.Audio.VolumeMusic = (float)value;
        }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            Client.Audio.VolumeMusic = (float)this.CurrentValue;
            ClientMusicSystem.EnsureMusicPlaying();
        }
    }
}