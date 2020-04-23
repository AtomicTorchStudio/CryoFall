namespace AtomicTorch.CBND.CoreMod.ClientOptions.Video
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;

    public class VideoOptionScreenAspectRatio
        : ProtoOptionCombobox<
            VideoOptionsCategory,
            VideoOptionScreenAspectRatio.ScreenMode>
    {
        [NotPersistent]
        public enum ScreenMode : byte
        {
            [Description(CoreStrings.TitleModeAuto)]
            Auto,

            [Description("16:9 (PvP maximum view)")]
            Force16To9
        }

        public override ScreenMode DefaultEnumValue => ScreenMode.Auto;

        public override string Name => "Screen aspect ratio";

        public override IProtoOption OrderAfterOption
            => GetOption<VideoOptionScreenMode>();

        public override ScreenMode ValueProvider
        {
            get => GetMode(Client.Rendering.AspectRatioMax);
            set => Client.Rendering.AspectRatioMax = GetMaxAspectRatio(value);
        }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            Client.Rendering.AspectRatioMax = GetMaxAspectRatio(this.CurrentValue);
        }

        private static double GetMaxAspectRatio(ScreenMode value)
        {
            switch (value)
            {
                case ScreenMode.Force16To9:
                    return 1920 / 1080.0;

                default:
                    // crop to ~21:9 max
                    // don't allow to crop further as otherwise vertical view area will be severely limited
                    return 2560 / 1080.0;
            }
        }

        private static ScreenMode GetMode(double aspectRatioMax)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (aspectRatioMax != 1920 / 1080.0)
            {
                return ScreenMode.Auto;
            }

            return ScreenMode.Force16To9;
        }
    }
}