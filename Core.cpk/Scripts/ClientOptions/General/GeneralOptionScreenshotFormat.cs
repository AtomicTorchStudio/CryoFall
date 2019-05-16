namespace AtomicTorch.CBND.CoreMod.ClientOptions.General
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class GeneralOptionScreenshotFormat
        : ProtoOptionCombobox<GeneralOptionsCategory,
            GeneralOptionScreenshotFormat.Format>
    {
        public enum Format : byte
        {
            [Description("PNG (lossless)")]
            Png = 0,

            [Description("JPEG")]
            Jpeg95 = 95 // JPEG with 95% quality
        }

        public override Format DefaultEnumValue => Format.Png;

        public override string Name => "Screenshot format";

        public override IProtoOption OrderAfterOption
            => GetOption<GeneralOptionUIScale>();

        public override Format ValueProvider { get; set; }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            var (format, quality) = this.GetFormat(this.CurrentValue);
            Api.Client.Rendering.ScreenshotFormat = format;
            Api.Client.Rendering.ScreenshotQuality = quality;
        }

        private (ScreenshotFormat format, float quality) GetFormat(Format format)
        {
            switch (format)
            {
                case Format.Png:
                    return (ScreenshotFormat.Png, 1);
                case Format.Jpeg95:
                    return (ScreenshotFormat.Jpeg, 0.95f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
    }
}