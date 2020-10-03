namespace AtomicTorch.CBND.CoreMod.Systems.ItemExplosive
{
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special.Base;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.SoundCue;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
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
            if (explosionPreset.ScreenShakesDuration > 0)
            {
                ClientComponentCameraScreenShakes.AddRandomShakes(
                    duration: explosionPreset.ScreenShakesDuration,
                    worldDistanceMin: explosionPreset.ScreenShakesWorldDistanceMin,
                    worldDistanceMax: explosionPreset.ScreenShakesWorldDistanceMax);
            }

            // play sound
            if (volume > 0)
            {
                var explosionSoundEmitter = Client.Audio.PlayOneShot(explosionPreset.SoundSet.GetSound(),
                                                                     worldPosition: position,
                                                                     volume: volume,
                                                                     pitch: RandomHelper.Range(0.95f, 1.05f));

                // extend explosion sound distance
                explosionSoundEmitter.CustomMinDistance = (float)explosionPreset.LightWorldSize / 3;
                explosionSoundEmitter.CustomMaxDistance = (float)explosionPreset.LightWorldSize;

                // add sound cues
                for (var i = 0; i < explosionPreset.SoundsCuesNumber; i++)
                {
                    ClientTimersSystem.AddAction(delaySeconds: i * 0.1,
                                                 () => ClientSoundCueManager.OnSoundEvent(position,
                                                                                          isPartyMember: false));
                }
            }

            // create explosion renderer
            var explosionSpriteAnimationDuration = explosionPreset.SpriteAnimationDuration;
            var explosionSceneObject = Client.Scene.CreateSceneObject("Temp explosion",
                                                                      position);

            explosionSceneObject.Destroy(delay: explosionSpriteAnimationDuration);

            var explosionSpriteRenderer = Client.Rendering.CreateSpriteRenderer(
                explosionSceneObject,
                TextureResource.NoTexture,
                drawOrder: explosionPreset.SpriteDrawOrder,
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
            if (explosionPreset.LightDuration > 0)
            {
                var explosionLightSceneObject = Client.Scene.CreateSceneObject("Temp explosion light",
                                                                               position);

                explosionLightSceneObject.Destroy(delay: explosionPreset.LightDuration);

                var explosionLight = ClientLighting.CreateLightSourceSpot(
                    explosionLightSceneObject,
                    explosionPreset.LightColor,
                    size: explosionPreset.LightWorldSize,
                    positionOffset: (0, 0),
                    spritePivotPoint: (0.5, 0.5));

                ClientComponentOneShotLightAnimation.Setup(
                    explosionLight,
                    explosionPreset.LightDuration);
            }

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

                                                var sizeX = MathHelper.Lerp(explosionPreset.BlastwaveSizeFrom.X,
                                                                            explosionPreset.BlastwaveSizeTo.X,
                                                                            alpha);
                                                var sizeY = MathHelper.Lerp(explosionPreset.BlastwaveSizeFrom.Y,
                                                                            explosionPreset.BlastwaveSizeTo.Y,
                                                                            alpha);
                                                blastSpriteRenderer.Size = (sizeX, sizeY);
                                            });
                    });
            }

            ClientGroundExplosionAnimationHelper.OnExplode(
                delaySeconds: explosionSpriteAnimationDuration / 2,
                position: position);
        }

        public static void ServerExplode(
            [CanBeNull] ICharacter character,
            [CanBeNull] IProtoExplosive protoExplosive,
            [CanBeNull] IProtoItemWeapon protoWeapon,
            ExplosionPreset explosionPreset,
            Vector2D epicenterPosition,
            DamageDescription damageDescriptionCharacters,
            IPhysicsSpace physicsSpace,
            ExecuteExplosionDelegate executeExplosionCallback)
        {
            ValidateIsServer();

            // schedule explosion charred ground spawning
            var protoObjectCharredGround = explosionPreset.ProtoObjectCharredGround;
            if (protoObjectCharredGround is not null)
            {
                ServerTimersSystem.AddAction(
                    delaySeconds: explosionPreset.SpriteAnimationDuration * 0.5,
                    () =>
                    {
                        var tilePosition = (Vector2Ushort)(epicenterPosition - protoObjectCharredGround.Layout.Center);
                        var canSpawnCharredGround = true;

                        var tile = Server.World.GetTile(tilePosition);
                        if (tile.ProtoTile.Kind != TileKind.Solid
                            || tile.EightNeighborTiles.Any(t => t.ProtoTile.Kind != TileKind.Solid))
                        {
                            // allow charred ground only on solid ground
                            canSpawnCharredGround = false;
                        }

                        if (canSpawnCharredGround)
                        {
                            // remove existing charred ground objects at the same tile
                            foreach (var staticWorldObject in Shared.WrapInTempList(
                                                                        tile.StaticObjects)
                                                                    .EnumerateAndDispose())
                            {
                                switch (staticWorldObject.ProtoStaticWorldObject)
                                {
                                    case ProtoObjectCharredGround _:
                                        Server.World.DestroyObject(staticWorldObject);
                                        break;

                                    case IProtoObjectDeposit _:
                                        // don't show charred ground over resource deposits (it looks wrong)
                                        canSpawnCharredGround = false;
                                        break;
                                }
                            }
                        }

                        if (canSpawnCharredGround
                            && PveSystem.ServerIsPvE)
                        {
                            var bounds = protoObjectCharredGround.Layout.Bounds;
                            if (LandClaimSystem.SharedIsLandClaimedByAnyone(
                                    new RectangleInt(tilePosition, bounds.Size + (1,1))))
                            {
                                // ensure that it's not possible to create charred ground in a land claim area in PvE
                                canSpawnCharredGround = false;
                            }
                        }

                        if (canSpawnCharredGround)
                        {
                            // spawn charred ground
                            var objectCharredGround =
                                Server.World.CreateStaticWorldObject(protoObjectCharredGround,
                                                                     tilePosition);
                            var objectCharredGroundOffset = epicenterPosition - tilePosition.ToVector2D();
                            if (objectCharredGroundOffset != Vector2D.Zero)
                            {
                                ProtoObjectCharredGround.ServerSetWorldOffset(objectCharredGround,
                                                                              (Vector2F)objectCharredGroundOffset);
                            }
                        }
                    });
            }

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
                                                                protoWeapon: protoWeapon,
                                                                protoAmmo: null,
                                                                damageDescription: damageDescriptionCharacters,
                                                                protoExplosive: protoExplosive);

                    // execute explosion
                    executeExplosionCallback(
                        positionEpicenter: epicenterPosition,
                        physicsSpace: physicsSpace,
                        weaponFinalCache: weaponFinalCache);
                });
        }
    }
}