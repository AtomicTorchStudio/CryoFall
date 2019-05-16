namespace AtomicTorch.CBND.CoreMod.ClientOptions.Video
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class VideoOptionRenderingResolutionScale
        : ProtoOptionCombobox<
            VideoOptionsCategory,
            VideoOptionRenderingResolutionScale.Mode>
    {
        protected internal static readonly IRenderingClientService Rendering
            = Api.IsClient
                  ? Api.Client.Rendering
                  : null;

        [NotPersistent]
        public enum Mode : byte
        {
            // We don't use these modes because they require a custom downsizing shader
            // (preferably bicubic) which is not implemented.
            //[Description("Super x4 (extremely slow)")]
            //Upscale4 = 25,

            //[Description("Super x2 (very slow)")]
            //Upscale2 = 50,

            [Description("Original (best quality)")]
            Normal = 100,

            [Description("Downscale x1.5 (faster)")]
            Downscale15 = 175,

            [Description("Downscale x2 (fastest)")]
            Downscale2 = 200
        }

        public override Mode DefaultEnumValue => Mode.Normal;

        public override string Name => "Rendering resolution";

        public override IProtoOption OrderAfterOption
            => GetOption<VideoOptionUIAntiAliasing>();

        public override Mode ValueProvider
        {
            get => ConvertViewportScaleToMode(Rendering.MainComposerViewportScale)
                   ?? this.Default;
            set
            {
                var scale = this.GetScaleFromMode(value);
                Rendering.MainComposerViewportScale = scale;
            }
        }

        private static Mode? ConvertViewportScaleToMode(double scale)
        {
            switch (scale)
            {
                //case 4:
                //    return Mode.Upscale4;

                //case 2:
                //    return Mode.Upscale2;

                case 1:
                    return Mode.Normal;

                case 0.75:
                    return Mode.Downscale15;

                case 0.5:
                    return Mode.Downscale2;

                default:
                    return null;
            }
        }

        private double GetScaleFromMode(Mode mode)
        {
            switch (mode)
            {
                //case Mode.Upscale4:
                //    return 4.0;

                //case Mode.Upscale2:
                //    return 2.0;

                case Mode.Normal:
                    return 1.0;

                case Mode.Downscale15:
                    return 0.75;

                case Mode.Downscale2:
                    return 0.5;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}