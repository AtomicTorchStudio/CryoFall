namespace AtomicTorch.CBND.CoreMod.ClientOptions.Video
{
    using System.ComponentModel;

    public class VideoOptionAntiAliasing
        : ProtoOptionCombobox<
            VideoOptionsCategory,
            VideoOptionAntiAliasing.AntiAliasingMode>
    {
        public enum AntiAliasingMode : byte
        {
            [Description("None")]
            None = 0,

            [Description("MSAA x2")]
            MSAAx2 = 2,

            [Description("MSAA x4")]
            MSAAx4 = 4,

            [Description("MSAA x8")]
            MSAAx8 = 8
        }

        public override AntiAliasingMode DefaultEnumValue => AntiAliasingMode.MSAAx8;

        public override string Name => "Anti-aliasing";

        public override IProtoOption OrderAfterOption
            => GetProtoEntity<VideoOptionFrameRateLimit>();

        public override AntiAliasingMode ValueProvider
        {
            get => (AntiAliasingMode)Client.Rendering.AntiAliasingMode;
            set => Client.Rendering.AntiAliasingMode = (GameApi.ServicesClient.Rendering.AntiAliasingMode)value;
        }
    }
}