namespace AtomicTorch.CBND.CoreMod.ClientOptions.Audio
{
    using AtomicTorch.CBND.CoreMod.Systems.Skills;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class AudioOptionVolumeMaster : ProtoOptionSlider<AudioOptionsCategory>
    {
        public override double Default => 1;

        public override double Maximum => 1;

        public override double Minimum => 0;

        public override string Name => "Master volume";

        public override double ValueProvider
        {
            get => Client.Audio.VolumeMaster;
            set => Client.Audio.VolumeMaster = (float)value;
        }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            Client.Audio.VolumeMaster = (float)this.CurrentValue;

            if (fromUi)
            {
                // play dummy sound just to check the volume
                Api.Client.Audio.PlayOneShot(
                    ClientComponentSkillsWatcher.SoundResourceSkillDiscovered);
            }
        }
    }
}