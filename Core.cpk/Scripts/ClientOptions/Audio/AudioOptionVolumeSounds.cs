namespace AtomicTorch.CBND.CoreMod.ClientOptions.Audio
{
    using AtomicTorch.CBND.CoreMod.Systems.Skills;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class AudioOptionVolumeSounds : ProtoOptionSlider<AudioOptionsCategory>
    {
        public override double Default => 1;

        public override double Maximum => 1;

        public override double Minimum => 0;

        public override string Name => "Sound volume";

        public override IProtoOption OrderAfterOption
            => GetProtoEntity<AudioOptionVolumeMaster>();

        public override double ValueProvider
        {
            get => Client.Audio.VolumeSounds;
            set => Client.Audio.VolumeSounds = (float)value;
        }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            Client.Audio.VolumeSounds = (float)this.CurrentValue;

            if (fromUi)
            {
                // play dummy sound just to check the volume
                Api.Client.Audio.PlayOneShot(
                    ClientComponentSkillsWatcher.SoundResourceSkillDiscovered);
            }
        }
    }
}