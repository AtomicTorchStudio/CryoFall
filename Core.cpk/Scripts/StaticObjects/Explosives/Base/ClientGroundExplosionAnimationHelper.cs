namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientGroundExplosionAnimationHelper
    {
        public const double ExplosionGroundDuration = 1.6;

        public static readonly ITextureAtlasResource ExplosionGroundTextureAtlas
            = new TextureAtlasResource("FX/Explosions/ExplosionGround1",
                                       columns: 8,
                                       rows: 3,
                                       isTransparent: true);

        private static readonly HashSet<Vector2Ushort> CurrentExplosions = new HashSet<Vector2Ushort>();

        public static void OnExplode(double delaySeconds, Vector2D position)
        {
            var tilePosition = position.ToVector2Ushort();
            CurrentExplosions.Add(tilePosition);

            ClientTimersSystem.AddAction(
                delaySeconds + ExplosionGroundDuration,
                () => CurrentExplosions.Remove(tilePosition));
        }

        public static bool HasActiveExplosion(Vector2Ushort tilePosition)
        {
            return CurrentExplosions.Contains(tilePosition);
        }

        public static bool IsGroundSpriteFlipped(Vector2Ushort tilePosition)
        {
            return PositionalRandom.Get(tilePosition,
                                        minInclusive: 0,
                                        maxExclusive: 2,
                                        seed: 218936133)
                   == 0;
        }

        public static void PreloadContent()
        {
            // preload the ground explosion spritesheet
            Api.Client.Rendering.PreloadTextureAsync(ExplosionGroundTextureAtlas);

            // preload the charred ground texture
            Api.Client.Rendering.PreloadTextureAsync(
                Api.GetProtoEntity<ObjectCharredGround>().DefaultTexture);
        }
    }
}