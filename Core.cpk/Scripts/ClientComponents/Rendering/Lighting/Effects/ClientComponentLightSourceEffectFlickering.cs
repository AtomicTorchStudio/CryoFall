namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentLightSourceEffectFlickering : ClientComponent
    {
        private double currentSizeX;

        private double currentSizeY;

        private BaseClientComponentLightSource lightSource;

        private Vector2D maxSizeChangePerSecond;

        private Interval<double> sizeRangeX;

        private Interval<double> sizeRangeY;

        private double targetSizeX;

        private double targetSizeY;

        private double timeUpdateAccumulator;

        private double updateInterval;

        public void Setup(
            BaseClientComponentLightSource lightSource,
            double flickeringPercents,
            double flickeringChangePercentsPerSecond,
            double updateRatePerSecond = 15)
        {
            if (flickeringPercents <= 0
                || flickeringPercents > 100)
            {
                throw new ArgumentException("Flickering percent must be in range (0;100]", nameof(flickeringPercents));
            }

            this.lightSource = lightSource;

            var size = lightSource.RenderingSize;

            this.sizeRangeX = new Interval<double>(
                size.X * (1 - flickeringPercents / 100.0),
                size.X);

            this.sizeRangeY = new Interval<double>(
                size.Y * (1 - flickeringPercents / 100.0),
                size.Y);

            this.maxSizeChangePerSecond = (size.X * flickeringChangePercentsPerSecond / 100.0,
                                           size.Y * flickeringChangePercentsPerSecond / 100.0);

            this.updateInterval = 1.0 / updateRatePerSecond;

            this.Randomize();
            this.currentSizeX = this.targetSizeX;
            this.currentSizeY = this.targetSizeY;
            this.Apply(deltaTime: 10000);
        }

        public override void Update(double deltaTime)
        {
            if (this.lightSource.IsDestroyed)
            {
                this.Destroy();
                return;
            }

            this.timeUpdateAccumulator += deltaTime;
            if (this.timeUpdateAccumulator < this.updateInterval)
            {
                this.Apply(deltaTime);
                return;
            }

            // time to update
            this.timeUpdateAccumulator %= this.updateInterval;
            this.Randomize();
            this.Apply(deltaTime);
        }

        private void Apply(double deltaTime)
        {
            this.currentSizeX = MathHelper.LerpTowards(
                this.currentSizeX,
                this.targetSizeX,
                this.maxSizeChangePerSecond.X * deltaTime);

            this.currentSizeY = MathHelper.LerpTowards(
                this.currentSizeY,
                this.targetSizeY,
                this.maxSizeChangePerSecond.Y * deltaTime);

            this.lightSource.RenderingSize = new Size2F((float)this.currentSizeX, (float)this.currentSizeY);
        }

        private void Randomize()
        {
            var random = RandomHelper.NextDouble();
            this.targetSizeX = this.sizeRangeX.Min + random * (this.sizeRangeX.Max - this.sizeRangeX.Min);
            this.targetSizeY = this.sizeRangeY.Min + random * (this.sizeRangeY.Max - this.sizeRangeY.Min);
        }
    }
}