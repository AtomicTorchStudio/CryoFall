namespace AtomicTorch.CBND.CoreMod.ClientOptions.General
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class GeneralOptionScreenshotResolution
        : ProtoOptionCombobox<GeneralOptionsCategory,
            GeneralOptionScreenshotResolution.Mode>
    {
        public enum Mode : byte
        {
            [Description("Actual (use current resolution)")]
            Actual = 0,

            /// <summary>
            /// This mode will round current resolution to the closest standard resolution (matching is done by the game engine).
            /// Especially useful when making screenshots in the windowed mode.
            /// For example, 1900x800 will be rounded to 1920x1080, 3000x2000 wil be rounded to 3840x2160, etc.
            /// This mode is used by default at it produces screenshots which are better to share.
            /// </summary>
            [Description("Closest (use closest standard resolution)")]
            Closest = 1,

            [NotLocalizable]
            [Description("1920x1080 (FHD)")]
            SizeFHD = 10,

            [NotLocalizable]
            [Description("2560x1440 (QHD)")]
            SizeQHD = 15,

            [NotLocalizable]
            [Description("3840x2160 (4K)")]
            Size4K = 20,

            // It's a super high quality mode but rarely useful for most players,
            // also it's quite slow (a few seconds lag to produce a screenshot)
            [NotLocalizable]
            [Description("7680x4320 (8K)")]
            Size8K = 30
        }

        public override Mode DefaultEnumValue => Mode.Size4K;

        public override string Name => "Screenshot resolution";

        public override IProtoOption OrderAfterOption => GetProtoEntity<GeneralOptionScreenshotFormat>();

        public override Mode ValueProvider { get; set; }

        protected override void OnCurrentValueChanged(bool fromUi)
        {
            Api.Client.Rendering.ScreenshotResolution = this.GetSize(this.CurrentValue);
        }

        private Vector2Ushort GetSize(Mode mode)
        {
            switch (mode)
            {
                case Mode.Actual:
                    // the game knows how to handle this magic number
                    return Vector2Ushort.Zero;

                case Mode.Closest:
                    // the game knows how to handle this magic number
                    return Vector2Ushort.Max;

                case Mode.SizeFHD:
                    return new Vector2Ushort(1920, 1080);

                case Mode.SizeQHD:
                    return new Vector2Ushort(2560, 1440);

                case Mode.Size4K:
                    return new Vector2Ushort(3840, 2160);

                case Mode.Size8K:
                    return new Vector2Ushort(7680, 4320);

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}