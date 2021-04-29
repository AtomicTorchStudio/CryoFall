namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ComponentWeaponTrace : ClientComponent
    {
        private IComponentSpriteRenderer componentSpriteRender;

        private double currentTime;

        private double fireDistance;

        private bool hasHit;

        private WeaponHitData hitData;

        private Vector2D normalizedRay;

        private double totalDuration;

        private WeaponFireTracePreset weaponTracePreset;

        private Vector2D worldPositionSource;

        public ComponentWeaponTrace() : base(isLateUpdateEnabled: true)
        {
        }

        public static void CalculateAngleAndDirection(
            Vector2D deltaPos,
            out double angleRad,
            out Vector2D normalizedRay)
        {
            angleRad = -Math.Atan2(deltaPos.Y, deltaPos.X);
            normalizedRay = new Vector2D(Math.Cos(-angleRad),
                                         Math.Sin(-angleRad));
        }

        public static void Create(
            WeaponFireTracePreset weaponTracePreset,
            Vector2D worldPositionSource,
            Vector2D endPosition,
            WeaponHitData lastHitData,
            bool hasHit)
        {
            if (weaponTracePreset is null)
            {
                // no weapon trace for this weapon
                return;
            }

            var deltaPos = endPosition - worldPositionSource;
            var fireDistance = CalculateFireDistance(weaponTracePreset, deltaPos);
            if (fireDistance <= weaponTracePreset.TraceMinDistance)
            {
                return;
            }

            CalculateAngleAndDirection(deltaPos,
                                       out var angleRad,
                                       out var normalizedRay);

            // offset start position of the ray
            worldPositionSource += normalizedRay * weaponTracePreset.TraceStartWorldOffset;

            // actual trace life duration is larger when has a hit
            // (to provide a contact fade-out animation for the sprite length)
            if (!hasHit)
            {
                // otherwise it's shorter on the sprite length
                fireDistance -= weaponTracePreset.TraceWorldLength;
            }

            var totalDuration = WeaponSystemClientDisplay.SharedCalculateTimeToHit(fireDistance, weaponTracePreset);

            var sceneObject = Api.Client.Scene.CreateSceneObject("Temp_WeaponTrace");
            var componentSpriteRender = Api.Client.Rendering.CreateSpriteRenderer(
                sceneObject,
                weaponTracePreset.TraceTexture,
                positionOffset: Vector2D.Zero,
                spritePivotPoint: (0, 0.5),
                // yes, it's actually making the weapon trace to draw in the light layer!
                drawOrder: DrawOrder.Light);

            componentSpriteRender.RotationAngleRad = (float)angleRad;
            componentSpriteRender.BlendMode = weaponTracePreset.UseScreenBlending
                                                  ? BlendMode.Screen
                                                  // it's important to use premultiplied mode here for correct rendering
                                                  : BlendMode.AlphaBlendPremultiplied;

            sceneObject.AddComponent<ComponentWeaponTrace>()
                       .Setup(weaponTracePreset,
                              componentSpriteRender,
                              worldPositionSource,
                              normalizedRay,
                              fireDistance,
                              totalDuration,
                              hasHit,
                              lastHitData);

            sceneObject.Destroy(totalDuration);
        }

        public override void LateUpdate(double deltaTime)
        {
            this.UpdateDirectionRayAndDistance();

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

            //ClientComponentPhysicsSpaceVisualizer.VisualizeTestResults(
            //        new List<Vector2D>() {this.SceneObject.Position}, CollisionGroups.HitboxRanged, isClient: true );

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

        private static double CalculateFireDistance(WeaponFireTracePreset weaponTracePreset, Vector2D deltaPos)
        {
            var fireDistance = deltaPos.Length;
            fireDistance = Math.Max(0, fireDistance);

            // extend fire distance by start offset (yes, we're using `-` here as TraceStartWorldOffset expected to be negative)
            fireDistance -= weaponTracePreset.TraceStartWorldOffset;
            return fireDistance;
        }

        private void Setup(
            WeaponFireTracePreset weaponTracePreset,
            IComponentSpriteRenderer componentSpriteRender,
            Vector2D worldPositionSource,
            Vector2D normalizedRay,
            double fireDistance,
            double totalDuration,
            bool hasHit,
            WeaponHitData hitData)
        {
            this.weaponTracePreset = weaponTracePreset;
            this.componentSpriteRender = componentSpriteRender;
            this.worldPositionSource = worldPositionSource;
            this.normalizedRay = normalizedRay;
            this.fireDistance = fireDistance;
            this.totalDuration = totalDuration;
            this.hasHit = hasHit;
            this.hitData = hitData;

            this.LateUpdate(0);
        }

        // If the hit was detected, guide the projectile towards the current hit target location.
        // For example, when other players firing into the current player
        // and the current player has large ping, it will look like the shots are hitting empty space but damage is dealt.
        // This method will guide projectiles to ensure there is no such issue.
        private void UpdateDirectionRayAndDistance()
        {
            if (!this.hasHit
                || !(this.hitData.WorldObject is IDynamicWorldObject dynamicWorldObject))
            {
                return;
            }

            var endPosition = dynamicWorldObject.Position + this.hitData.HitPoint;
            var deltaPos = endPosition - this.worldPositionSource;
            this.fireDistance = CalculateFireDistance(this.weaponTracePreset, deltaPos);

            CalculateAngleAndDirection(deltaPos,
                                       out var angleRad,
                                       out this.normalizedRay);

            this.componentSpriteRender.RotationAngleRad = (float)angleRad;
        }
    }
}