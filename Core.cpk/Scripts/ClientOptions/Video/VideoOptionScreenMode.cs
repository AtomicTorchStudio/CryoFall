namespace AtomicTorch.CBND.CoreMod.ClientOptions.Video
{
    using System.ComponentModel;

    public class VideoOptionScreenMode
        : ProtoOptionCombobox<
            VideoOptionsCategory,
            VideoOptionScreenMode.ScreenMode>
    {
        public enum ScreenMode : byte
        {
            [Description("Fullscreen")]
            Fullscreen,

            [Description("Windowed")]
            Windowed
        }

        public override ScreenMode DefaultEnumValue => ScreenMode.Fullscreen;

        public override string Name => "Screen mode";

        public override ScreenMode ValueProvider
        {
            get => Client.Rendering.IsFullscreen ? ScreenMode.Fullscreen : ScreenMode.Windowed;
            set => Client.Rendering.IsFullscreen = value == ScreenMode.Fullscreen;
        }
    }
}