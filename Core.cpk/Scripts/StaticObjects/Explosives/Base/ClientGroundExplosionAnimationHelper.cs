namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientGroundExplosionAnimationHelper
    {
        public static readonly ITextureAtlasResource ExplosionGroundTextureAtlas
            = new TextureAtlasResource("FX/Explosions/ExplosionGround1",
                                       columns: 8,
                                       rows: 3,
                                       isTransparent: true);

        public static double ExplosionGroundDuration = 1.6;

        private static readonly HashSet<Vector2Ushort> CurrentExplosions = new HashSet<Vector2Ushort>();

        public static void Explode(double delaySeconds, Vector2D position)
        {
            var tilePosition = position.ToVector2Ushort();
            CurrentExplosions.Add(tilePosition);

            ClientTimersSystem.AddAction(
                delaySeconds,
                () =>
                {
                    var groundSceneObject = Api.Client.Scene.CreateSceneObject("Temp explosion ground",
                                                                               position);

                    var groundSpriteRenderer = Api.Client.Rendering.CreateSpriteRenderer(
                        groundSceneObject,
                        TextureResource.NoTexture,
                        drawOrder: DrawOrder.FloorCharredGround,
                        spritePivotPoint: (0.5, 0.5),
                        scale: ObjectCharredGround.Scale);

                    if (IsGroundSpriteFlipped(tilePosition))
                    {
                        groundSpriteRenderer.DrawMode = DrawMode.FlipHorizontally;
                    }

                    var duration = ExplosionGroundDuration;
                    ClientComponentOneShotSpriteSheetAnimationHelper.Setup(
                        groundSpriteRenderer,
                        ExplosionGroundTextureAtlas,
                        duration);

                    groundSceneObject.Destroy(duration);

                    ClientTimersSystem.AddAction(
                        duration,
                        () => CurrentExplosions.Remove(tilePosition));
                });
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