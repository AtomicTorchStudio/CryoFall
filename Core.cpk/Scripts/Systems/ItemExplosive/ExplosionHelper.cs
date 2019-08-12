namespace AtomicTorch.CBND.CoreMod.Systems.ItemExplosive
{
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;
    using static GameApi.Scripting.Api;

    public static class ExplosionHelper
    {
        public static readonly EffectResource EffectResourceAdditiveColorEffect
            = new EffectResource("AdditiveColorEffect");

        public delegate void ExecuteExplosionDelegate(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            WeaponFinalCache weaponFinalCache);

        public static void ClientExplode(
            Vector2D position,
            ExplosionPreset explosionPreset,
            float volume = 1.0f)
        {
            // add screen shakes
            ClientComponentCameraScreenShakes.AddRandomShakes(
                duration: explosionPreset.ScreenShakesDuration,
                worldDistanceMin: explosionPreset.ScreenShakesWorldDistanceMin,
                worldDistanceMax: explosionPreset.ScreenShakesWorldDistanceMax);

            // play sound
            var explosionSoundEmitter = Client.Audio.PlayOneShot(explosionPreset.SoundSet.GetSound(),
                                                                 worldPosition: position,
                                                                 volume: volume,
                                                                 pitch: RandomHelper.Range(0.95f, 1.05f));

            // extend explosion sound distance
            explosionSoundEmitter.CustomMinDistance = (float)explosionPreset.LightWorldSize / 3;
            explosionSoundEmitter.CustomMaxDistance = (float)explosionPreset.LightWorldSize;

            // create explosion renderer
            var explosionSpriteAnimationDuration = explosionPreset.SpriteAnimationDuration;
            var explosionSceneObject = Client.Scene.CreateSceneObject("Temp explosion",
                                                                      position);

            explosionSceneObject.Destroy(delay: explosionSpriteAnimationDuration);

            var explosionSpriteRenderer = Client.Rendering.CreateSpriteRenderer(
                explosionSceneObject,
                TextureResource.NoTexture,
                drawOrder: DrawOrder.Explosion,
                spritePivotPoint: (0.5, 0.5));

            explosionSpriteRenderer.Color = explosionPreset.SpriteColorMultiplicative;
            explosionSpriteRenderer.Size = explosionPreset.SpriteSize;
            if (explosionPreset.SpriteColorAdditive.HasValue
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                || explosionPreset.SpriteBrightness != 1)
            {
                var renderingMaterial = RenderingMaterial.Create(EffectResourceAdditiveColorEffect);
                renderingMaterial.EffectParameters
                                 .Set("ColorAdditive", explosionPreset.SpriteColorAdditive ?? Colors.Black)
                                 .Set("Brightness",    explosionPreset.SpriteBrightness);
                explosionSpriteRenderer.RenderingMaterial = renderingMaterial;
            }

            var isFlipped = 0
                            == PositionalRandom.Get(position.ToVector2Ushort(),
                                                    minInclusive: 0,
                                                    maxExclusive: 2,
                                                    seed: 893243289);
            if (isFlipped)
            {
                explosionSpriteRenderer.DrawMode = DrawMode.FlipHorizontally;
            }

            ClientComponentOneShotSpriteSheetAnimationHelper.Setup(
                explosionSpriteRenderer,
                explosionPreset.SpriteAtlasResources.TakeByRandom(),
                explosionSpriteAnimationDuration);

            // add light source for the explosion
            var explosionLight = ClientLighting.CreateLightSourceSpot(
                explosionSceneObject,
                explosionPreset.LightColor,
                size: explosionPreset.LightWorldSize,
                positionOffset: (0, 0),
                spritePivotPoint: (0.5, 0.5));

            ClientComponentOneShotLightAnimation.Setup(
                explosionLight,
                explosionPreset.LightDuration);

            // add blast wave
            var blastAnimationDuration = explosionPreset.BlastwaveAnimationDuration;
            if (blastAnimationDuration > 0)
            {
                ClientTimersSystem.AddAction(
                    explosionPreset.BlastwaveDelay,
                    () =>
                    {
                        var blastSceneObject = Client.Scene.CreateSceneObject("Temp explosion",
                                                                              position);

                        blastSceneObject.Destroy(delay: blastAnimationDuration);

                        var blastSpriteRenderer = Client.Rendering.CreateSpriteRenderer(
                            blastSceneObject,
                            new TextureResource("FX/ExplosionBlast"),
                            drawOrder: DrawOrder.Explosion - 1,
                            spritePivotPoint: (0.5, 0.5));

                        // animate blast wave
                        ClientComponentGenericAnimationHelper.Setup(
                            blastSceneObject,
                            blastAnimationDuration,
                            updateCallback: alpha =>
                                            {
                                                var blastwaveAlpha = (byte)(byte.MaxValue * (1 - alpha));
                                                blastSpriteRenderer.Color = explosionPreset.BlastWaveColor
                                                                                           .WithAlpha(blastwaveAlpha);

                                                var sizeX = MathHelper.Lerp((float)explosionPreset.BlastwaveSizeFrom.X,
                                                                            (float)explosionPreset.BlastwaveSizeTo.X,
                                                                            alpha);
                                                var sizeY = MathHelper.Lerp((float)explosionPreset.BlastwaveSizeFrom.Y,
                                                                            (float)explosionPreset.BlastwaveSizeTo.Y,
                                                                            alpha);
                                                blastSpriteRenderer.Size = new Size2F(sizeX, sizeY);
                                            });
                    });
            }

            // add ground explosion animation
            {
                ClientGroundExplosionAnimationHelper.Explode(
                    delaySeconds: explosionSpriteAnimationDuration / 2,
                    position: position);
            }
        }

        public static void ServerExplode(
            [CanBeNull] ICharacter character,
            [CanBeNull] IProtoObjectExplosive protoObjectExplosive,
            ExplosionPreset explosionPreset,
            Vector2D epicenterPosition,
            DamageDescription damageDescriptionCharacters,
            IPhysicsSpace physicsSpace,
            ExecuteExplosionDelegate executeExplosionCallback)
        {
            ValidateIsServer();

            // schedule explosion charred ground spawning
            ServerTimersSystem.AddAction(
                // the delay is quite small and just needed to ensure
                // that the charred ground spawned some time after this object is destroyed
                delaySeconds: explosionPreset.SpriteAnimationDuration * 0.5,
                () =>
                {
                    var tilePosition = (Vector2Ushort)epicenterPosition;
                    if (!Server.World.GetTile(tilePosition)
                               .StaticObjects
                               .Any(so => so.ProtoStaticWorldObject is ObjectCharredGround))
                    {
                        // spawn charred ground
                        var objectCharredGround =
                            Server.World.CreateStaticWorldObject<ObjectCharredGround>(tilePosition);
                        var objectCharredGroundOffset = epicenterPosition - tilePosition.ToVector2D();
                        if (objectCharredGroundOffset != Vector2D.Zero)
                        {
                            ObjectCharredGround.ServerSetWorldOffset(objectCharredGround,
                                                                     objectCharredGroundOffset.ToVector2F());
                        }
                    }
                });

            // schedule explosion damage
            ServerTimersSystem.AddAction(
                delaySeconds: explosionPreset.ServerDamageApplyDelay,
                () =>
                {
                    // prepare weapon caches
                    var characterFinalStatsCache = character?.SharedGetFinalStatsCache()
                                                   ?? FinalStatsCache.Empty;

                    var weaponFinalCache = new WeaponFinalCache(character,
                                                                characterFinalStatsCache,
                                                                weapon: null,
                                                                protoWeapon: null,
                                                                protoObjectExplosive: protoObjectExplosive,
                                                                damageDescription: damageDescriptionCharacters);

                    // execute explosion
                    executeExplosionCallback(
                        positionEpicenter: epicenterPosition,
                        physicsSpace: physicsSpace,
                        weaponFinalCache: weaponFinalCache);
                });
        }
    }
}