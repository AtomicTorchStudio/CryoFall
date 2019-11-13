namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ComponentWeaponTrace : ClientComponent
    {
        private IComponentSpriteRenderer componentSpriteRender;

        private double currentTime;

        private double fireDistance;

        private bool hasHit;

        private Vector2D normalizedRay;

        private double totalDuration;

        private WeaponFireTracePreset weaponTracePreset;

        private Vector2D worldPositionSource;

        public ComponentWeaponTrace() : base(isLateUpdateEnabled: true)
        {
        }

        public static double CalculateTimeToHit(
            WeaponFireTracePreset weaponTracePreset,
            Vector2D worldPositionSource,
            Vector2D endPosition)
        {
            if (weaponTracePreset is null)
            {
                // no weapon trace for this weapon
                return 0;
            }

            var deltaPos = endPosition - worldPositionSource;
            var fireDistance = deltaPos.Length;
            // hit happens when the end of the weapon trace sprite touches the target, so cut the distance accordingly

            fireDistance -= weaponTracePreset.TraceWorldLength;
            fireDistance = Math.Max(0, fireDistance);
            return CalculateTimeToHit(fireDistance, weaponTracePreset);
        }

        public static void Create(
            WeaponFireTracePreset weaponTracePreset,
            Vector2D worldPositionSource,
            Vector2D endPosition,
            bool hasHit)
        {
            if (weaponTracePreset is null)
            {
                // no weapon trace for this weapon
                return;
            }

            var deltaPos = endPosition - worldPositionSource;
            var fireDistance = deltaPos.Length;
            fireDistance = Math.Max(0, fireDistance);
            if (fireDistance <= weaponTracePreset.TraceWorldLength)
            {
                return;
            }

            // actual trace life duration is larger when has a hit
            // (to provide a contact fade-out animation for the sprite length)
            if (!hasHit)
            {
                // otherwise it's shorter on the sprite length
                fireDistance -= weaponTracePreset.TraceWorldLength;
            }

            var totalDuration = CalculateTimeToHit(fireDistance, weaponTracePreset);

            var sceneObject = Api.Client.Scene.CreateSceneObject("Temp_WeaponTrace");
            var componentSpriteRender = Api.Client.Rendering.CreateSpriteRenderer(
                sceneObject,
                weaponTracePreset.TraceTexture,
                positionOffset: Vector2D.Zero,
                spritePivotPoint: (0, 0.5),
                // yes, it's actually making the weapon trace to draw in the light layer!
                drawOrder: DrawOrder.Light);

            var angleRad = -Math.Atan2(deltaPos.Y, deltaPos.X);
            componentSpriteRender.RotationAngleRad = (float)angleRad;

            var normalizedRay = new Vector2D(Math.Cos(-angleRad),
                                             Math.Sin(-angleRad));

            // offset start position of the ray
            worldPositionSource += normalizedRay * weaponTracePreset.TraceStartWorldOffset;
            fireDistance -= weaponTracePreset.TraceStartWorldOffset; // extend fire distance accordingly

            sceneObject.AddComponent<ComponentWeaponTrace>()
                       .Setup(weaponTracePreset,
                              componentSpriteRender,
                              worldPositionSource,
                              normalizedRay,
                              fireDistance,
                              totalDuration,
                              hasHit);

            sceneObject.Destroy(totalDuration);
        }

        public override void LateUpdate(double deltaTime)
        {
            var spriteWorldLength = this.weaponTracePreset.TraceWorldLength;

            this.currentTime += deltaTime;
            this.currentTime = Math.Min(this.currentTime, this.totalDuration);
            var timeFraction = this.currentTime / this.totalDuration;

            // to ensure the sprite will not start behind the muzzle, let's "decompress" it from the muzzle as time goes on
            {
                var distance = this.fireDistance * timeFraction;
                var scaleIn = 1.0;

                if (distance < spriteWorldLength)
                {
                    scaleIn = distance / spriteWorldLength;
                    scaleIn = Math.Pow(scaleIn, this.weaponTracePreset.TraceStartScaleSpeedExponent);
                }

                var scaleOut = 1.0;
                if (this.hasHit)
                {
                    var distanceRemains = this.fireDistance - distance;
                    if (distanceRemains < spriteWorldLength)
                    {
                        scaleOut = distanceRemains / spriteWorldLength;
                    }
                }

                var scale = Math.Min(scaleIn, scaleOut);
                this.componentSpriteRender.Scale = (scale, 1);
            }

            var currentDistance = this.fireDistance * timeFraction;
            this.SceneObject.Position = this.worldPositionSource
                                        + this.normalizedRay * currentDistance;

            if (this.hasHit)
            {
                return;
            }

            // no hit, fade out the trace by the end (when reaching the max range)
            byte colorOpacity = 0xFF;
            var rangeRemains = this.fireDistance - currentDistance;
            if (rangeRemains < spriteWorldLength)
            {
                var opacityCoef = rangeRemains / spriteWorldLength;
                opacityCoef = Math.Min(opacityCoef, 1);
                opacityCoef = Math.Pow(opacityCoef, this.weaponTracePreset.TraceEndFadeOutExponent);
                colorOpacity = (byte)(0xFF * opacityCoef);
            }

            this.componentSpriteRender.Color = Color.FromArgb(colorOpacity, 0xFF, 0xFF, 0xFF);
        }

        private static double CalculateTimeToHit(double fireDistance, WeaponFireTracePreset weaponTracePreset)
        {
            return fireDistance / weaponTracePreset.TraceSpeed;
        }

        private void Setup(
            WeaponFireTracePreset weaponTracePreset,
            IComponentSpriteRenderer componentSpriteRender,
            Vector2D worldPositionSource,
            Vector2D normalizedRay,
            double fireDistance,
            double totalDuration,
            bool hasHit)
        {
            this.weaponTracePreset = weaponTracePreset;
            this.componentSpriteRender = componentSpriteRender;
            this.worldPositionSource = worldPositionSource;
            this.normalizedRay = normalizedRay;
            this.fireDistance = fireDistance;
            this.totalDuration = totalDuration;
            this.hasHit = hasHit;

            this.LateUpdate(0);
        }
    }
}