namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ComponentWeaponEnergyBeam : ClientComponent
    {
        private static readonly EffectResource BeamEffectResource
            = new("Special/WeaponLaserBeam");

        private readonly RenderingMaterial renderingMaterial 
            = RenderingMaterial.Create(BeamEffectResource);

        private double accumulatedTime;

        private double alpha;

        private Vector2D beamOriginOffset;

        private double beamWidth;

        private double duration;

        private Vector2D primaryRendererDefaultPositionOffset;

        private IComponentSpriteRenderer spriteRendererLine;

        private Vector2D targetPosition;

        public static void Create(
            Vector2D sourcePosition,
            Vector2D targetPosition,
            double traceStartWorldOffset,
            ITextureResource texture,
            double beamWidth,
            Vector2D originOffset,
            double duration,
            bool endsWithHit,
            double fadeInDistance,
            double fadeOutDistanceHit,
            double fadeOutDistanceNoHit,
            BlendMode blendMode)
        {
            var deltaPos = targetPosition - sourcePosition;
            var length = deltaPos.Length;
            var fadeInFraction = fadeInDistance / length;
            var fadeOutFraction = endsWithHit
                                      ? fadeOutDistanceHit / length
                                      : fadeOutDistanceNoHit / length;

            if (fadeInFraction > 0.333
                || fadeOutFraction > 0.333)
            {
                // no sense in displaying such a short beam
                return;
            }

            var sceneObject = Client.Scene.CreateSceneObject(nameof(ComponentWeaponEnergyBeam));
            var component = sceneObject.AddComponent<ComponentWeaponEnergyBeam>();

            ComponentWeaponTrace.CalculateAngleAndDirection(deltaPos,
                                                            out var angleRad,
                                                            out var normalizedRay);

            // offset start position of the ray
            sourcePosition += normalizedRay * traceStartWorldOffset;

            sceneObject.Position = sourcePosition;

            component.beamOriginOffset = originOffset;
            component.beamWidth = beamWidth;
            component.primaryRendererDefaultPositionOffset = Vector2D.Zero;
            component.spriteRendererLine.TextureResource = texture;
            component.targetPosition = targetPosition;
            component.duration = duration;

            component.renderingMaterial
                     .EffectParameters
                     .Set("Length", (float)length)
                     .Set("FadeInFraction",  (float)fadeInFraction)
                     .Set("FadeOutFraction", (float)fadeOutFraction);

            component.spriteRendererLine.BlendMode = blendMode;

            component.Update(0);
        }

        public static void PreloadAssets()
        {
            Client.Rendering.PreloadEffectAsync(BeamEffectResource);
        }

        public override void Update(double deltaTime)
        {
            this.accumulatedTime += deltaTime;
            if (this.accumulatedTime > this.duration)
            {
                this.SceneObject.Destroy();
                return;
            }

            this.alpha = 1 - this.accumulatedTime / this.duration;

            // fade in-out version
            //if (this.alpha > 0.5)
            //{
            //    this.alpha = 1 - this.alpha;
            //}
            //this.alpha *= 2;

            this.renderingMaterial.EffectParameters.Set("Opacity", (float)this.alpha);

            // update line start-end positions and rotation angle
            var currentBeamOriginOffset = this.beamOriginOffset
                                          - this.primaryRendererDefaultPositionOffset;

            var lineStartWorldPosition = this.SceneObject.Position + currentBeamOriginOffset;

            // ReSharper disable once PossibleInvalidOperationException
            var lineEndWorldPosition = this.targetPosition;
            var lineDirection = lineEndWorldPosition - lineStartWorldPosition;

            this.spriteRendererLine.PositionOffset = currentBeamOriginOffset;
            this.spriteRendererLine.RotationAngleRad = (float)-Math.Atan2(lineDirection.Y, lineDirection.X);
            this.spriteRendererLine.Size = (ScriptingConstants.TileSizeVirtualPixels * lineDirection.Length,
                                            ScriptingConstants.TileSizeVirtualPixels * this.beamWidth);
        }

        protected override void OnEnable()
        {
            this.spriteRendererLine = Api.Client.Rendering.CreateSpriteRenderer(
                this.SceneObject,
                textureResource: null,
                spritePivotPoint: (0, 0.5),
                drawOrder: DrawOrder.Light);
            this.spriteRendererLine.RenderingMaterial = this.renderingMaterial;
        }
    }
}