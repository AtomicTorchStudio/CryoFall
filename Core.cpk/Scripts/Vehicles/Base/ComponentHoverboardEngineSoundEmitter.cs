namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Input.ClientPrediction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ComponentHoverboardEngineSoundEmitter : ClientComponent
    {
        // The last vehicle position is stored in order to calculate
        // the approximated vehicle velocity if the vehicle is not simulated locally.
        private Vector2D lastVehiclePosition;

        private IComponentSoundEmitter soundEmitter;

        private SoundResource soundResourceEngine;

        private IDynamicWorldObject vehicle;

        private VehiclePublicState vehiclePublicState;

        public float DistanceMax { get; set; } = 11;

        public float DistanceMin { get; set; } = 2;

        public float MaxPitch { get; set; } = 2.5f;

        public double PitchInterpolationRate { get; set; } = 10;

        public IDynamicWorldObject Vehicle => this.vehicle;

        public double VelocityCoef { get; set; } = 0.12;

        public double Volume { get; set; }

        public double VolumeInterpolationRate { get; set; } = 10;

        public void Setup(
            IDynamicWorldObject vehicle,
            SoundResource soundResourceEngine,
            double volume)
        {
            this.vehicle = vehicle;
            this.soundResourceEngine = soundResourceEngine;
            this.Volume = volume;

            this.ReleaseSubscriptions();
            this.vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
            this.vehiclePublicState.ClientSubscribe(p => p.PilotCharacter,
                                                    this.RebuildSoundEmitter,
                                                    this);

            this.RebuildSoundEmitter();
        }

        public override void Update(double deltaTime)
        {
            if (this.soundEmitter is null
                || this.vehicle.IsDestroyed)
            {
                if (this.vehicle.IsDestroyed)
                {
                    this.IsEnabled = false;
                }

                return;
            }

            var isLoading = LoadingSplashScreenManager.Instance.CurrentState == LoadingSplashScreenState.Shown;
            if (isLoading)
            {
                if (this.soundEmitter.State == SoundEmitterState.Playing)
                {
                    this.soundEmitter.Pause();
                }

                return;
            }

            this.SceneObject.Position = this.vehicle.Position;

            Vector2D velocity;
            var pilot = this.vehiclePublicState.PilotCharacter;
            if ((pilot?.IsCurrentClientCharacter ?? false)
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

            float targetPitch = 1;
            if (velocity != Vector2D.Zero)
            {
                targetPitch += (float)(velocity.Length * this.VelocityCoef);
                targetPitch = Math.Min(targetPitch, this.MaxPitch);
            }

            if (this.soundEmitter.State == SoundEmitterState.Playing)
            {
                // interpolate pitch to target value
                this.soundEmitter.Pitch = (float)MathHelper.LerpWithDeltaTime(
                    a: this.soundEmitter.Pitch,
                    b: targetPitch,
                    Api.Client.Core.DeltaTime,
                    rate: this.PitchInterpolationRate);
            }
            else
            {
                this.soundEmitter.Pitch = targetPitch;
                this.soundEmitter.Play();
            }

            this.soundEmitter.CustomMinDistance = this.DistanceMin;
            this.soundEmitter.CustomMaxDistance = this.DistanceMax;

            var targetVolume = pilot is null || this.vehicle.IsDestroyed
                                   ? 0
                                   : this.Volume;

            var volume = MathHelper.LerpWithDeltaTime(
                a: this.soundEmitter.Volume,
                b: targetVolume,
                Api.Client.Core.DeltaTime,
                rate: this.VolumeInterpolationRate);
            this.soundEmitter.Volume = (float)volume;

            if (targetVolume <= 0
                && volume <= 0.0001)
            {
                this.DestroySoundEmitter();
            }

            //// uncomment to log sound info
            //if (Api.Client.Core.ClientFrameNumber % 10 == 0)
            //{
            //    Logger.Dev($"Velocity: {velocity} | pitch: {this.soundEmitter.Pitch:F2} | targetPitch: {targetPitch:F2} | volume: {volume:F2}");
            //}
        }

        protected override void OnDisable()
        {
            HoverboardEngineSoundEmittersManager.OnSoundEmitterComponentVehicleDestroyed(this);
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

        private void DestroySoundEmitter()
        {
            this.soundEmitter?.Destroy();
            this.soundEmitter = null;
        }

        private void RebuildSoundEmitter()
        {
            var pilot = this.vehiclePublicState.PilotCharacter;
            if (pilot is null)
            {
                // sound emitter would be automatically destroyed as there is no pilot
                return;
            }

            this.DestroySoundEmitter(); // ensure sound emitter is destroyed as we're creating a new emitter
            this.lastVehiclePosition = this.vehicle.Position;

            var protoVehicle = (IProtoVehicle)this.vehicle.ProtoGameObject;
            this.soundEmitter = Client.Audio.CreateSoundEmitter(
                this.SceneObject,
                this.soundResourceEngine,
                is3D: !pilot.IsCurrentClientCharacter,
                isLooped: true,
                radius: protoVehicle.ObjectSoundRadius,
                volume: 0,
                isPlaying: false);

            this.Update(0);
        }
    }
}