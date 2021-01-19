namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class MuzzleFlashDescription : IMuzzleFlashDescriptionReadOnly
    {
        public Color LightColor { get; set; }

        public double LightDurationSeconds { get; set; }

        public double LightPower { get; set; }

        public Vector2D LightScreenOffsetRelativeToTexture { get; set; }

        public double TextureAnimationDurationSeconds { get; set; }

        public TextureAtlasResource TextureAtlas { get; set; }

        public double TextureScale { get; set; }

        public Vector2D TextureScreenOffset { get; set; }

        public MuzzleFlashDescription Clone()
        {
            return new()
            {
                LightColor = this.LightColor,
                LightDurationSeconds = this.LightDurationSeconds,
                LightPower = this.LightPower,
                LightScreenOffsetRelativeToTexture = this.LightScreenOffsetRelativeToTexture,
                TextureAnimationDurationSeconds = this.TextureAnimationDurationSeconds,
                TextureAtlas = this.TextureAtlas,
                TextureScale = this.TextureScale,
                TextureScreenOffset = this.TextureScreenOffset
            };
        }

        public MuzzleFlashDescription Set(IMuzzleFlashDescriptionReadOnly preset)
        {
            this.LightColor = preset.LightColor;
            this.LightDurationSeconds = preset.LightDurationSeconds;
            this.LightPower = preset.LightPower;
            this.LightScreenOffsetRelativeToTexture = preset.LightScreenOffsetRelativeToTexture;
            this.TextureAnimationDurationSeconds = preset.TextureAnimationDurationSeconds;
            this.TextureAtlas = preset.TextureAtlas;
            this.TextureScale = preset.TextureScale;
            this.TextureScreenOffset = preset.TextureScreenOffset;
            return this;
        }

        public MuzzleFlashDescription Set(
            Color? lightColor = null,
            double? lightDurationSeconds = null,
            double? lightPower = null,
            Vector2D? lightRelativeToTextureScreenOffset = null,
            double? textureAnimationDurationSeconds = null,
            TextureAtlasResource textureAtlas = null,
            double? textureScale = null,
            Vector2D? textureScreenOffset = null)
        {
            if (lightColor.HasValue)
            {
                this.LightColor = lightColor.Value;
            }

            if (lightDurationSeconds is not null)
            {
                this.LightDurationSeconds = lightDurationSeconds.Value;
            }

            if (lightPower is not null)
            {
                this.LightPower = lightPower.Value;
            }

            if (lightRelativeToTextureScreenOffset is not null)
            {
                this.LightScreenOffsetRelativeToTexture = lightRelativeToTextureScreenOffset.Value;
            }

            if (textureAnimationDurationSeconds is not null)
            {
                this.TextureAnimationDurationSeconds = textureAnimationDurationSeconds.Value;
            }

            if (textureAtlas is not null)
            {
                this.TextureAtlas = textureAtlas;
            }

            if (textureScale is not null)
            {
                this.TextureScale = textureScale.Value;
            }

            if (textureScreenOffset is not null)
            {
                this.TextureScreenOffset = textureScreenOffset.Value;
            }

            return this;
        }
    }
}