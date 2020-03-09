namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.Special;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.StatusEffects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentLightSourceEffectPulsating : ClientComponent
    {
        private double durationSeconds;

        private bool isIncreasingValue;

        private BaseClientComponentLightSource lightSource;

        private PostEffectPragmiumSourceExplosion postEffect;

        private double postEffectIntensityMultiplier;

        private double safeDistance;

        private Interval<double> sizeRangeX;

        private Interval<double> sizeRangeY;

        private IComponentSoundEmitter soundEmitter;

        private double soundVolume;

        private double value;

        private IStaticWorldObject worldObject;

        public void Setup(
            IStaticWorldObject worldObject,
            double safeDistance,
            BaseClientComponentLightSource lightSource,
            IComponentSoundEmitter soundEmitter,
            double fromPercents,
            double toPercents,
            double durationSeconds)
        {
            if (fromPercents < 0)
            {
                throw new ArgumentException("fromPercent must be >= 0");
            }

            if (fromPercents >= toPercents)
            {
                throw new ArgumentException("toPercent must be higher than fromPercent");
            }

            if (durationSeconds <= 0)
            {
                throw new Exception("Duration must be > 0");
            }

            this.worldObject = worldObject;
            this.safeDistance = safeDistance;
            this.durationSeconds = durationSeconds;
            this.lightSource = lightSource;
            this.soundEmitter = soundEmitter;

            var size = lightSource.RenderingSize;

            this.sizeRangeX = new Interval<double>(
                size.X * (fromPercents / 100.0),
                size.X * (toPercents / 100.0));

            this.sizeRangeY = new Interval<double>(
                size.Y * (fromPercents / 100.0),
                size.Y * (toPercents / 100.0));

            this.value = (this.lightSource.RenderingSize.X - this.sizeRangeX.Min)
                         / (this.sizeRangeX.Max - this.sizeRangeX.Min);
            this.isIncreasingValue = true;
            this.Apply(deltaTime: 0);
        }

        public override void Update(double deltaTime)
        {
            if (this.lightSource.IsDestroyed)
            {
                this.Destroy();
                return;
            }

            this.Apply(deltaTime);
        }

        protected override void OnDisable()
        {
            this.postEffect.Destroy();
            this.postEffect = null;
        }

        protected override void OnEnable()
        {
            this.postEffect = ClientPostEffectsManager.Add<PostEffectPragmiumSourceExplosion>();
            this.postEffect.Intensity = this.postEffectIntensityMultiplier = 0;
        }

        private void Apply(double deltaTime)
        {
            var worldObjectIsDestroyed = this.worldObject.IsDestroyed;
            var isSafeDistance = this.IsSafeDistance();

            // we're multiplying on 2 here because the duration is for the whole interval (back+forward)
            var deltaValue = 2 * deltaTime / this.durationSeconds;

            if (this.isIncreasingValue)
            {
                this.value += deltaValue;
                if (this.value >= 1)
                {
                    this.value = 1;
                    // start decreasing value
                    this.isIncreasingValue = false;
                }
            }
            else
            {
                this.value -= deltaValue;
                if (this.value <= 0)
                {
                    this.value = 0;

                    if (!worldObjectIsDestroyed)
                    {
                        // start increasing value
                        this.isIncreasingValue = true;
                    }
                }
            }

            this.postEffectIntensityMultiplier = MathHelper.LerpWithDeltaTime(
                this.postEffectIntensityMultiplier,
                isSafeDistance ? 0 : 1,
                deltaTime,
                rate: 10);

            this.soundVolume = MathHelper.LerpWithDeltaTime(
                this.soundVolume,
                worldObjectIsDestroyed ? 0 : 1,
                deltaTime,
                rate: 5);

            this.postEffect.Intensity = this.value * this.postEffectIntensityMultiplier;
            this.soundEmitter.Volume = (float)this.soundVolume;
            
            var sizeX = this.sizeRangeX.Min + this.value * (this.sizeRangeX.Max - this.sizeRangeX.Min);
            var sizeY = this.sizeRangeY.Min + this.value * (this.sizeRangeY.Max - this.sizeRangeY.Min);

            if (worldObjectIsDestroyed)
            {
                sizeX *= this.postEffectIntensityMultiplier;
                sizeY *= this.postEffectIntensityMultiplier;
            }

            this.lightSource.RenderingSize = new Size2F((float)sizeX, (float)sizeY);

            if (worldObjectIsDestroyed
                && this.value < 0.001
                && this.postEffectIntensityMultiplier < 0.001
                && this.soundVolume < 0.001)
            {
                this.SceneObject.Destroy();
            }
        }

        private bool IsSafeDistance()
        {
            if (this.worldObject.IsDestroyed)
            {
                return true;
            }

            var character = ClientCurrentCharacterHelper.Character.Position;
            var distance = character.DistanceTo(this.worldObject.TilePosition.ToVector2D()
                                                + this.worldObject.ProtoStaticWorldObject.Layout.Center);

            return distance > this.safeDistance;
        }
    }
}