namespace AtomicTorch.CBND.CoreMod.Noise
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// Some code ported from https://github.com/SebLague/Procedural-Landmass-Generation (MIT license)
    /// </summary>
    public class PerlinNoise : ISimplexNoise
    {
        private readonly DelegateCombineNoise combineFunction;

        private readonly ISimplexNoise noise;

        private readonly Vector2F[] octaveOffsets;

        private readonly PerlinNoiseSettings settings;

        public PerlinNoise(PerlinNoiseSettings settings)
        {
            this.noise = Api.Shared.CreateSimplexNoise(settings.Seed);
            this.settings = settings;

            // generate octave offsets
            this.octaveOffsets = new Vector2F[settings.Octaves];
            var random = new Random(settings.Seed);

            for (var i = 0; i < settings.Octaves; i++)
            {
                var offsetX = random.Next(-100000, 100000); // + settings.Offset.X;
                var offsetY = random.Next(-100000, 100000); // - settings.Offset.Y;
                this.octaveOffsets[i] = (offsetX, offsetY);
            }

            this.combineFunction = settings.GetCombineFunction();
        }

        public PerlinNoise(
            int seed,
            double scale,
            int octaves,
            double persistance,
            double lacunarity)
            : this(new PerlinNoiseSettings(
                       seed,
                       scale,
                       octaves,
                       persistance,
                       lacunarity))
        {
        }

        public PerlinNoiseSettings Settings => this.settings;

        public double CombineWith(double current, double value)
        {
            return this.combineFunction(current, value);
        }

        public double Get(double x, double y)
        {
            double amplitude = 1,
                   frequency = 1,
                   result = 0;

            for (var i = 0; i < this.settings.Octaves; i++)
            {
                var sampleX = (x + this.octaveOffsets[i].X) / this.settings.Scale * frequency;
                var sampleY = (y + this.octaveOffsets[i].Y) / this.settings.Scale * frequency;

                var rawNoise = this.noise.Get(sampleX, sampleY) * 2 - 1;
                result += rawNoise * amplitude;

                amplitude *= this.settings.Persistance;
                frequency *= this.settings.Lacunarity;
            }

            result = (result + 1) / 2;
            return MathHelper.Clamp(result, 0, 1);
        }

        public double Get(double x, double y, double z)
        {
            throw new NotImplementedException();
        }
    }
}