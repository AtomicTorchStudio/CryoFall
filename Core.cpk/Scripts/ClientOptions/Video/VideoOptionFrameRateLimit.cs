namespace AtomicTorch.CBND.CoreMod.ClientOptions.Video
{
    using System.ComponentModel;

    public class VideoOptionFrameRateLimit
        : ProtoOptionCombobox<
            VideoOptionsCategory,
            VideoOptionFrameRateLimit.LimitMode>
    {
        public enum LimitMode : byte
        {
            [Description("V-sync (recommended)")]
            VSync = 1,

            [Description("To screen refresh rate")]
            DisplayRefreshRate = 10,

            [Description("Unlimited (very high power draw)")]
            Off = 255
        }

        public override LimitMode DefaultEnumValue => LimitMode.VSync;

        public override string Name => "FPS limit";

        public override IProtoOption OrderAfterOption
            => GetProtoEntity<VideoOptionScreenMode>();

        public override LimitMode ValueProvider
        {
            get
            {
                if (Client.Rendering.IsVSyncEnabled)
                {
                    return LimitMode.VSync;
                }

                return Client.Rendering.FrameRateLimit.HasValue
                           ? LimitMode.DisplayRefreshRate
                           : LimitMode.Off;
            }
            set
            {
                bool vsync;
                ushort? fpsLimit;
                switch (value)
                {
                    case LimitMode.VSync:
                    default:
                        vsync = true;
                        fpsLimit = 0; // limit frame rate to the display rate
                        break;

                    case LimitMode.DisplayRefreshRate:
                        vsync = false;
                        fpsLimit = 0; // limit frame rate to the display rate
                        break;

                    case LimitMode.Off:
                        vsync = false;
                        fpsLimit = null; // unlimited frame rate
                        break;
                }

                Client.Rendering.IsVSyncEnabled = vsync;
                Client.Rendering.FrameRateLimit = fpsLimit;
            }
        }
    }
}