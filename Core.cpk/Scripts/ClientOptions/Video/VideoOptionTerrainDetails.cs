namespace AtomicTorch.CBND.CoreMod.ClientOptions.Video
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class VideoOptionTerrainDetails
        : ProtoOptionCombobox<
            VideoOptionsCategory,
            VideoOptionTerrainDetails.Mode>
    {
        private const Mode DefaultMode = Mode.High;

        public enum Mode : byte
        {
            [Description("High (recommended)")]
            High = 0,

            [Description("Medium")]
            Medium = 10,

            [Description("Low")]
            Low = 20,

            [Description("None (not recommended)")]
            None = 100,
        }

        public static Mode CurrentMode { get; private set; }

        public static double TerrainDecalNoiseSelectorRangeMultiplier { get; private set; }
            = GetNoiseSelectorRangeMultiplier(DefaultMode);

        public override Mode DefaultEnumValue => DefaultMode;

        public override string Name => "Terrain details";

        public override IProtoOption OrderAfterOption
            => GetOption<VideoOptionSpriteResolution>();

        public override Mode ValueProvider
        {
            get => CurrentMode;
            set
            {
                // ensure that the range multiplier is assigned
                TerrainDecalNoiseSelectorRangeMultiplier = GetNoiseSelectorRangeMultiplier(value);

                if (CurrentMode == value)
                {
                    return;
                }

                CurrentMode = value;
                Api.Client.Rendering.RefreshAllTileRenderers();
            }
        }

        private static double GetNoiseSelectorRangeMultiplier(Mode mode)
        {
            switch (mode)
            {
                case Mode.High:
                    return 1;

                case Mode.Medium:
                    return 0.5;

                case Mode.Low:
                    return 0.25;

                case Mode.None:
                    return 0;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}