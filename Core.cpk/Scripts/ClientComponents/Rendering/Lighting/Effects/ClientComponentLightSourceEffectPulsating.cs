namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentLightSourceEffectPulsating : ClientComponent
    {
        private double durationSeconds;

        private bool isIncreasingValue;

        private BaseClientComponentLightSource lightSource;

        private Interval<double> sizeRangeX;

        private Interval<double> sizeRangeY;

        private double value;

        public void Setup(
            BaseClientComponentLightSource lightSource,
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

            this.durationSeconds = durationSeconds;
            this.lightSource = lightSource;

            var size = lightSource.RenderingSize;

            this.sizeRangeX = new Interval<double>(
                size.X * (fromPercents / 100.0),
                size.X * (toPercents / 100.0));

            this.sizeRangeY = new Interval<double>(
                size.Y * (fromPercents / 100.0),
                size.Y * (toPercents / 100.0));

            this.value = (this.lightSource.RenderingSize.X - this.sizeRangeX.Min) / (this.sizeRangeX.Max - this.sizeRangeX.Min);
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

        private void Apply(double deltaTime)
        {
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
                    // start increasing value
                    this.isIncreasingValue = true;
                }
            }

            var sizeX = this.sizeRangeX.Min + this.value * (this.sizeRangeX.Max - this.sizeRangeX.Min);
            var sizeY = this.sizeRangeY.Min + this.value * (this.sizeRangeY.Max - this.sizeRangeY.Min);
            this.lightSource.RenderingSize = new Size2F((float)sizeX, (float)sizeY);
        }
    }
}