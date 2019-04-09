namespace AtomicTorch.CBND.CoreMod.ClientOptions.Audio
{
    using AtomicTorch.CBND.GameApi.Scripting;

    public class AudioOptionMuteAudioWhenUnfocused : ProtoOptionCheckbox<AudioOptionsCategory>
    {
        public override bool Default => false;

        public override string Name => "Mute when unfocused";

        public override bool ValueProvider
        {
            get => Api.Client.Audio.IsMuteAudioWhenUnfocused;
            set => Api.Client.Audio.IsMuteAudioWhenUnfocused = value;
        }
    }
}