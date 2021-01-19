namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IMuzzleFlashDescriptionReadOnly
    {
        Color LightColor { get; }

        double LightDurationSeconds { get; }

        double LightPower { get; }

        Vector2D LightScreenOffsetRelativeToTexture { get; }

        double TextureAnimationDurationSeconds { get; }

        TextureAtlasResource TextureAtlas { get; }

        double TextureScale { get; }

        Vector2D TextureScreenOffset { get; }

        MuzzleFlashDescription Clone();
    }
}