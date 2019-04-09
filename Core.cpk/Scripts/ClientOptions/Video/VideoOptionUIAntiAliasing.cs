namespace AtomicTorch.CBND.CoreMod.ClientOptions.Video
{
    using System.ComponentModel;

    public class VideoOptionUIAntiAliasing
        : ProtoOptionCombobox<
            VideoOptionsCategory,
            VideoOptionUIAntiAliasing.UIAntiAliasingMode>
    {
        public enum UIAntiAliasingMode : byte
        {
            [Description("Force PPAA (low)")]
            PPAA = 1,

            [Description("Auto MSAA (best)")]
            MSAA = 2,
        }

        public override UIAntiAliasingMode DefaultEnumValue => UIAntiAliasingMode.MSAA;

        /// <summary>
        /// We've decided to hide this option as it's misleading.
        /// PPAA anti-aliasing actually requires extra resources,
        /// but when MSAA is disabled it's the only way to have some UI AA.
        /// MSAA enabled automatically when global anti-aliasing is enabled.
        /// PPAA enabled automatically when MSAA is not available.
        /// </summary>
        public override bool IsHidden => true;

        public override string Name => "UI anti-aliasing";

        public override IProtoOption OrderAfterOption
            => GetProtoEntity<VideoOptionAntiAliasing>();

        public override UIAntiAliasingMode ValueProvider
        {
            get => (UIAntiAliasingMode)Client.Rendering.UIAntiAliasingMode;
            set => Client.Rendering.UIAntiAliasingMode = (GameApi.ServicesClient.Rendering.UIAntiAliasingMode)value;
        }
    }
}