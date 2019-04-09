namespace AtomicTorch.CBND.CoreMod.ClientOptions.Audio
{
    using AtomicTorch.CBND.CoreMod.ClientOptions.Video;

    public class AudioOptionsCategory : ProtoOptionsCategory
    {
        public override string Name => "Audio";

        public override ProtoOptionsCategory OrderAfterCategory => GetProtoEntity<VideoOptionsCategory>();

        protected override void OnApply()
        {
        }

        protected override void OnReset()
        {
        }
    }
}