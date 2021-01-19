namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolGenerator
{
    using System;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class IslandGeneratorAlgorithm : BaseEditorToolGeneratorAlgorithm
    {
        public override ITextureResource Icon { get; } = new TextureResource("Editor/ToolGenerator/GeneratorIsland");

        public override string Name => "Island";

        protected override void ClientGenerateMap(long seed, BoundsUshort worldBounds, byte startHeight)
        {
            EditorStaticObjectsRemovalHelper.ClientDelete(worldBounds);

            var map = GenerateIslandMap(seed, worldBounds.Size);
            this.ClientApplyMap(worldBounds.Offset, map, startHeight);

            EditorClientActionsHistorySystem.Purge();
        }

        private static double[,] GenerateIslandMap(long seed, Vector2Ushort size)
        {
            // the algorithm uses three simplex noise sources and combine their results together
            var noise1 = Api.Shared.CreateSimplexNoise(seed);
            var noise2 = Api.Shared.CreateSimplexNoise(seed + 1);
            var noise3 = Api.Shared.CreateSimplexNoise(seed + 2);

            var scale = 1024 / (double)size.X;

            var noiseCoefficient = 1d;
            var noise1Size = noiseCoefficient * 0.007;
            var noise2Size = noiseCoefficient * 0.0055;
            var noise3Size = noiseCoefficient * 0.003;

            noise1Size *= scale;
            noise2Size *= scale;
            noise3Size *= scale;

            var minThreshold = 0.3;
            var maxThreshold = 0.6;

            var map = new double[size.X, size.Y];

            for (var y = 0; y < size.Y; y++)
            for (var x = 0; x < size.X; x++)
            {
                double value;
                {
                    var value1 = GetNoiseValue(noise1, x, y, noise1Size);
                    var value2 = GetNoiseValue(noise2, x, y, noise2Size);
                    var value3 = GetNoiseValue(noise3, x, y, noise3Size);
                    value = (value1 + value2 + value3) / 3;
                }

                // apply radial mask
                {
                    var dx = 2 * (x / (double)size.X - 0.5);
                    var dy = 2 * (y / (double)size.Y - 0.5);
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    value *= 1 - distance * distance * distance;
                    if (value < 0)
                    {
                        value = 0;
                    }
                }

                // remap
                if (value <= minThreshold)
                {
                    value = 0;
                }
                else if (value >= maxThreshold)
                {
                    value = 1;
                }
                else
                {
                    value = (value - minThreshold) / (maxThreshold - minThreshold);
                    if (value > 1)
                    {
                        value = 1;
                    }
                }

                map[x, y] = value;
            }

            return map;
        }

        private static double GetNoiseValue(ISimplexNoise noise, int x, int y, double noiseScale)
        {
            return noise.Get(x * noiseScale, y * noiseScale);
        }
    }
}