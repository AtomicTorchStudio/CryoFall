namespace AtomicTorch.CBND.CoreMod.Drones
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDroneControl;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ComponentDroneMiningBeam : ClientComponent
    {
        private const double HitSparksInterval = 0.03333;

        private const double MiningTargetEndPositionAnimationSpeed = 2;

        private const double MiningTargetEndPositionMaxDistance = 0.05;

        private const float MiningVolume = 0.65f;

        private static readonly RenderingMaterial BeamRenderingMaterial
            = RenderingMaterial.Create(
                new EffectResource("Special/MiningLaserBeam"));

        private static readonly SoundResource SourceResourceMiningEnd
            = new("Items/Drones/MiningEnd");

        private static readonly SoundResource SourceResourceMiningProcessStone
            = new("Items/Drones/MiningProcessStone");

        private static readonly SoundResource SourceResourceMiningProcessWood
            = new("Items/Drones/MiningProcessWood");

        private static readonly SoundResource SourceResourceMiningStart
            = new("Items/Drones/MiningStart");

        private double alpha;

        private Color beamColor;

        private Vector2D beamOriginOffset;

        private double beamWidth;

        private DronePublicState dronePublicState;

        private IStaticWorldObject hitWorldObject;

        private Vector2D? lastTargetPosition;

        private double miningTargetEndPositionAnimationAngle;

        private IComponentSpriteRenderer primaryRenderer;

        private Vector2D primaryRendererDefaultPositionOffset;

        private IComponentSoundEmitter soundEmitterMiningProcess;

        private IComponentSpriteRenderer spriteRendererLine;

        private IComponentSpriteRenderer spriteRendererOrigin;

        private double timeSinceLastHitSparks;

        public void Setup(
            DronePublicState dronePublicState,
            Color beamColor,
            double beamWidth,
            ITextureResource beamOriginTexture,
            ITextureResource beamTexture,
            Vector2D beamOriginOffset,
            IComponentSpriteRenderer primaryRenderer)
        {
            this.dronePublicState = dronePublicState;
            this.beamColor = beamColor;
            this.beamOriginOffset = beamOriginOffset;
            this.beamWidth = beamWidth;
            this.primaryRenderer = primaryRenderer;
            this.primaryRendererDefaultPositionOffset = primaryRenderer.PositionOffset;
            this.miningTargetEndPositionAnimationAngle = RandomHelper.NextDouble() * MathConstants.DoublePI;
            this.spriteRendererLine.TextureResource = beamTexture;
            this.spriteRendererOrigin.TextureResource = beamOriginTexture;
            this.Update(0);
        }

        public override void Update(double deltaTime)
        {
            if (this.dronePublicState.TargetObjectPosition.HasValue)
            {
                this.lastTargetPosition = this.dronePublicState.TargetObjectPosition.Value.ToVector2D();
                this.hitWorldObject ??= CharacterDroneControlSystem.SharedGetCompatibleTargetObject(
                    this.lastTargetPosition.Value.ToVector2Ushort());

                if (this.hitWorldObject is not null)
                {
                    var objectCenter = DroneTargetPositionHelper.GetTargetPosition(this.hitWorldObject);
                    objectCenter = (objectCenter.X, objectCenter.Y * 0.5 + 0.2);
                    this.lastTargetPosition += objectCenter;
                }
            }
            else
            {
                this.hitWorldObject = null;
            }

            if (this.hitWorldObject is not null
                && this.soundEmitterMiningProcess.SoundResource is null)
            {
                this.soundEmitterMiningProcess.SoundResource
                    = GetSourceResourceMiningProcess(this.hitWorldObject.ProtoStaticWorldObject);
            }

            var wasEnabled = this.spriteRendererLine.IsEnabled;
            var isBeamActive = this.dronePublicState.IsMining
                               && this.lastTargetPosition.HasValue;
            this.alpha = MathHelper.LerpWithDeltaTime(
                this.alpha,
                isBeamActive ? 1 : 0,
                deltaTime: deltaTime,
                rate: 20);

            var isEnabled = this.alpha > 0.001;
            this.spriteRendererLine.IsEnabled = isEnabled;
            this.spriteRendererOrigin.IsEnabled = isEnabled;
            this.soundEmitterMiningProcess.IsEnabled = isEnabled;

            if (!isEnabled)
            {
                if (wasEnabled)
                {
                    // just disabled
                    Api.Client.Audio.PlayOneShot(SourceResourceMiningEnd,
                                                 this.SceneObject,
                                                 MiningVolume);
                }

                return;
            }

            if (!wasEnabled)
            {
                // just enabled
                Api.Client.Audio.PlayOneShot(SourceResourceMiningStart,
                                             this.SceneObject,
                                             MiningVolume);
            }

            var alphaComponent = (byte)(this.alpha * byte.MaxValue);
            this.spriteRendererLine.Color = this.beamColor.WithAlpha(alphaComponent);
            this.spriteRendererOrigin.Color = Color.FromArgb(alphaComponent, 0xFF, 0xFF, 0xFF);
            this.soundEmitterMiningProcess.Volume = (float)(this.alpha * MiningVolume);

            // update line start-end positions and rotation angle
            var currentBeamOriginOffset = this.beamOriginOffset
                                          + this.primaryRenderer.PositionOffset
                                          - this.primaryRendererDefaultPositionOffset;

            var lineStartWorldPosition = this.SceneObject.Position + currentBeamOriginOffset;

            // ReSharper disable once PossibleInvalidOperationException
            var lineEndWorldPosition = this.lastTargetPosition.Value;
            lineEndWorldPosition += this.GetMiningTargetMovementAnimation(deltaTime);

            var lineDirection = lineEndWorldPosition - lineStartWorldPosition;

            this.spriteRendererLine.PositionOffset = currentBeamOriginOffset;
            this.spriteRendererLine.RotationAngleRad = (float)-Math.Atan2(lineDirection.Y, lineDirection.X);
            this.spriteRendererLine.Scale = (lineDirection.Length, this.beamWidth);
            this.spriteRendererLine.DrawOrderOffsetY = this.primaryRenderer.DrawOrderOffsetY;

            this.spriteRendererOrigin.PositionOffset = currentBeamOriginOffset;
            this.spriteRendererOrigin.Scale = this.beamWidth;
            this.spriteRendererOrigin.DrawOrderOffsetY = this.primaryRenderer.DrawOrderOffsetY;

            if (this.hitWorldObject is null)
            {
                return;
            }

            this.timeSinceLastHitSparks -= deltaTime;
            if (this.timeSinceLastHitSparks < 0)
            {
                WeaponSystemClientDisplay.ClientAddHitSparks(
                    WeaponHitSparksPresets.LaserMining,
                    new WeaponHitData(Vector2D.Zero),
                    this.hitWorldObject,
                    this.hitWorldObject.ProtoStaticWorldObject,
                    worldObjectPosition: lineEndWorldPosition,
                    projectilesCount: 1,
                    objectMaterial: this.hitWorldObject.ProtoStaticWorldObject.SharedGetObjectMaterial(),
                    randomizeHitPointOffset: false,
                    randomRotation: true,
                    rotationAngleRad: null,
                    drawOrder: DrawOrder.Light + 2,
                    animationFrameDuration: 3 / 60.0);
                this.timeSinceLastHitSparks += HitSparksInterval;
            }
        }

        protected override void OnEnable()
        {
            this.spriteRendererLine = Api.Client.Rendering.CreateSpriteRenderer(
                this.SceneObject,
                textureResource: null,
                drawOrder: DrawOrder.Light,
                spritePivotPoint: (0, 0.5));
            this.spriteRendererLine.RenderingMaterial = BeamRenderingMaterial;
            this.spriteRendererLine.IsEnabled = false;

            this.spriteRendererOrigin = Api.Client.Rendering.CreateSpriteRenderer(
                this.SceneObject,
                textureResource: default,
                drawOrder: DrawOrder.Light + 1,
                spritePivotPoint: (0.5, 0.5));
            this.spriteRendererOrigin.IsEnabled = false;
            this.spriteRendererOrigin.BlendMode = BlendMode.AdditivePremultiplied;

            this.soundEmitterMiningProcess = Api.Client.Audio.CreateSoundEmitter(
                this.SceneObject,
                SoundResource.NoSound,
                isLooped: true,
                is3D: true);
            this.soundEmitterMiningProcess.IsEnabled = false;
        }

        private static SoundResource GetSourceResourceMiningProcess(IProtoStaticWorldObject protoStaticWorldObject)
        {
            return protoStaticWorldObject is IProtoObjectVegetation
                       ? SourceResourceMiningProcessWood
                       : SourceResourceMiningProcessStone;
        }

        private Vector2D GetMiningTargetMovementAnimation(double deltaTime)
        {
            this.miningTargetEndPositionAnimationAngle += deltaTime * MiningTargetEndPositionAnimationSpeed;
            return (MiningTargetEndPositionMaxDistance * Math.Cos(this.miningTargetEndPositionAnimationAngle),
                    MiningTargetEndPositionMaxDistance * Math.Sin(this.miningTargetEndPositionAnimationAngle));
        }
    }
}