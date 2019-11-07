namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Input.ClientPrediction;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ComponentHoverboardVisualManager : ClientComponent
    {
        private const double BaseDrawOrderOffsetY = -0.325;

        private const double HoverboardBaseOffset = -0.14;

        private const double MalfunctionLightFlickerRate = 50;

        private const double MalfunctionLightSourceMinOpacity = 0.95;

        private const double MalfunctionLightSpriteMinOpacity = 0.5;

        private const double OffsetOff = -0.05;

        private const double SpriteScale = 0.9;

        public double EffectHalfDistanceIdle = 0.015;

        // When moving, the hoverboard will float up-down faster and with a higher amplitude.
        public double EffectHalfDistanceMoving = 2.4 * 0.015;

        public double EffectSpeedIdle = 4;

        public double EffectSpeedMoving = 8;

        public double MaxVelocity = 3.0;

        // The last vehicle position is stored in order to calculate
        // the approximated vehicle velocity if the vehicle is not simulated locally.
        private Vector2D lastVehiclePosition;

        private BaseClientComponentLightSource lightSourceActiveEngine;

        private ICharacter pilotCharacter;

        private IComponentSpriteRenderer spriteRendererHoverboard;

        private IComponentSpriteRenderer spriteRendererLight;

        private double startTime;

        private IDynamicWorldObject vehicle;

        public ComponentHoverboardVisualManager() : base(isLateUpdateEnabled: true)
        {
        }

        private bool IsHoverboardMalfunctioning
            => ((IProtoVehicle)this.vehicle.ProtoGameObject)
               .SharedGetMoveSpeedMultiplier(
                   this.vehicle,
                   this.pilotCharacter)
               < 1;

        public override void LateUpdate(double deltaTime)
        {
            double offsetY;

            if (this.pilotCharacter == null)
            {
                // no pilot - hoverboard off
                offsetY = OffsetOff;
                this.spriteRendererHoverboard.DrawOrder = DrawOrder.Occlusion - 1;
                this.spriteRendererLight.IsEnabled = false;

                this.spriteRendererHoverboard.PositionOffset = (0, HoverboardBaseOffset + offsetY);
                this.spriteRendererHoverboard.DrawOrderOffsetY =
                    BaseDrawOrderOffsetY - this.spriteRendererHoverboard.PositionOffset.Y;
                return;
            }

            Vector2D velocity;
            if ((this.pilotCharacter?.IsCurrentClientCharacter ?? false)
                && ClientCurrentCharacterLagPredictionManager.IsLagPredictionEnabled)
            {
                // this vehicle is simulated on the client side so we can take its velocity directly
                velocity = this.vehicle.PhysicsBody.Velocity;
            }
            else // calculate approximated velocity by using the position difference
            {
                velocity = this.CalculateVelocity(deltaTime);
            }

            this.lastVehiclePosition = this.vehicle.Position;

            // has a pilot - hoverboard on
            var time = 10000.0 + ((Client.Core.ClientRealTime - this.startTime) % 10000.0);

            offsetY = MathHelper.Lerp(
                this.EffectHalfDistanceIdle * Math.Sin(time * this.EffectSpeedIdle + MathConstants.PI),
                this.EffectHalfDistanceMoving * Math.Sin(time * this.EffectSpeedMoving + MathConstants.PI),
                this.GetMovemementSpeedCoef(velocity));

            this.spriteRendererHoverboard.DrawOrder = DrawOrder.Default;

            if (!this.spriteRendererLight.IsEnabled)
            {
                // order is important - we need to enable hoverboard renderer after light renderer to draw it over
                this.spriteRendererHoverboard.IsEnabled = false;
                this.spriteRendererLight.IsEnabled = true;
                this.spriteRendererHoverboard.IsEnabled = true;
            }

            this.spriteRendererHoverboard.PositionOffset = (0, HoverboardBaseOffset + offsetY);
            this.spriteRendererLight.PositionOffset = (0, HoverboardBaseOffset + offsetY);

            this.spriteRendererHoverboard.DrawOrderOffsetY =
                BaseDrawOrderOffsetY - this.spriteRendererHoverboard.PositionOffset.Y;

            this.spriteRendererLight.DrawOrderOffsetY =
                BaseDrawOrderOffsetY - this.spriteRendererLight.PositionOffset.Y;

            double lightSourceOpacity = 1.0,
                   lightSpriteOpacity = 1.0;
            if (this.IsHoverboardMalfunctioning)
            {
                // vehicle is malfunctioning
                var alpha = (Math.Sin(time * MalfunctionLightFlickerRate) + 1) / 2;
                lightSourceOpacity = MalfunctionLightSourceMinOpacity + alpha * (1 - MalfunctionLightSourceMinOpacity);
                lightSpriteOpacity = MalfunctionLightSpriteMinOpacity + alpha * (1 - MalfunctionLightSpriteMinOpacity);
            }

            this.spriteRendererLight.Color = Color.FromArgb((byte)(0xFF * lightSpriteOpacity), 0xFF, 0xFF, 0xFF);
            this.lightSourceActiveEngine.Opacity = lightSourceOpacity;

            if (!this.pilotCharacter.IsInitialized)
            {
                return;
            }

            var pilotClientState = this.pilotCharacter.GetClientState<BaseCharacterClientState>();
            var skeletonPilot = pilotClientState.SkeletonRenderer;
            if (skeletonPilot is null)
            {
                return;
            }

            skeletonPilot.PositionOffset = (0, offsetY);
            skeletonPilot.DrawOrderOffsetY = BaseDrawOrderOffsetY - offsetY - 0.05;

            var pilotShadow = pilotClientState.RendererShadow;
            pilotShadow.DrawOrder = DrawOrder.Default;
            pilotShadow.PositionOffset = (0, offsetY);
            pilotShadow.DrawOrderOffsetY = BaseDrawOrderOffsetY - offsetY - 0.04;

            // ensure the pilot's position is perfectly synchronized with the hoverboard
            Client.World.SetPosition(this.pilotCharacter, this.vehicle.Position, forceReset: false);
        }

        public void Setup(
            IDynamicWorldObject vehicle,
            BaseClientComponentLightSource lightSourceActiveEngine,
            TextureResource textureResourceHoverboard,
            TextureResource textureResourceLight)
        {
            this.vehicle = vehicle;
            this.lightSourceActiveEngine = lightSourceActiveEngine;
            this.spriteRendererLight = Client.Rendering.CreateSpriteRenderer(
                this.SceneObject,
                textureResourceLight,
                drawOrder: DrawOrder.Default,
                positionOffset: (0, HoverboardBaseOffset),
                spritePivotPoint: (0.5, 0.5));
            this.spriteRendererLight.DrawOrderOffsetY = -HoverboardBaseOffset;

            this.spriteRendererHoverboard = Client.Rendering.CreateSpriteRenderer(
                this.SceneObject,
                textureResourceHoverboard,
                drawOrder: DrawOrder.Default,
                positionOffset: (0, HoverboardBaseOffset),
                spritePivotPoint: (0.5, 0.5));
            this.spriteRendererHoverboard.DrawOrderOffsetY = -HoverboardBaseOffset;

            this.spriteRendererLight.Scale = this.spriteRendererHoverboard.Scale = SpriteScale;

            var vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
            vehiclePublicState.ClientSubscribe(p => p.PilotCharacter,
                                               this.RefreshCurrentPilotCharacter,
                                               this);

            this.RefreshCurrentPilotCharacter(vehiclePublicState.PilotCharacter);
        }

        // calculate approximated velocity by using the position difference
        private Vector2D CalculateVelocity(double deltaTime)
        {
            if (deltaTime <= 0)
            {
                return Vector2D.Zero;
            }

            var currentPosition = this.vehicle.Position;
            var velocity = (currentPosition - this.lastVehiclePosition) / deltaTime;
            return velocity;
        }

        private double GetMovemementSpeedCoef(Vector2D velocity)
        {
            var length = velocity.Length;
            length /= this.MaxVelocity;
            return Math.Min(1, length);
        }

        private void RefreshCurrentPilotCharacter(ICharacter newPilotCharacter)
        {
            this.pilotCharacter = newPilotCharacter;
            this.startTime = Client.Core.ClientRealTime;
        }
    }
}