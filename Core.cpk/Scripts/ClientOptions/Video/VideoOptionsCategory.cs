namespace AtomicTorch.CBND.CoreMod.ClientOptions.Video
{
    using AtomicTorch.CBND.CoreMod.ClientOptions.General;

    public class VideoOptionsCategory : ProtoOptionsCategory
    {
        public override string Name => "Video";

        public override ProtoOptionsCategory OrderAfterCategory => GetProtoEntity<GeneralOptionsCategory>();

        protected override void OnApply()
        {
        }

        protected override void OnReset()
        {
        }
    }
}