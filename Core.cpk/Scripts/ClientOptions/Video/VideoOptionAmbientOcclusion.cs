namespace AtomicTorch.CBND.CoreMod.ClientOptions.Video
{
    public class VideoOptionAmbientOcclusion
        : ProtoOptionCheckbox<VideoOptionsCategory>
    {
        public static bool IsEnabled { get; private set; }

        // this is a very GPU-heavy feature which we prefer to have out-in now
        public override bool Default => false;

        public override string Name => "Ambient occlusion";

        public override IProtoOption OrderAfterOption
            => GetOption<VideoOptionTerrainDetails>();

        public override bool ValueProvider
        {
            get => IsEnabled;
            set => IsEnabled = value;
        }
    }
}