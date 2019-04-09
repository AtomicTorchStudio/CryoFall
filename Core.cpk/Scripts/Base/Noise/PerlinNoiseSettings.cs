namespace AtomicTorch.CBND.CoreMod.Noise
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using AtomicTorch.GameEngine.Common.Helpers;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class PerlinNoiseSettings
    {
        private static readonly char[] ParsingValueEndChar = { ',', ')' };

        public readonly NoiseCombineMode CombineMode;

        public readonly double Lacunarity;

        public readonly int Octaves;

        public readonly double Persistance;

        public readonly double Scale;

        public readonly int Seed;

        public PerlinNoiseSettings(
            int seed,
            double scale,
            int octaves,
            double persistance,
            double lacunarity,
            NoiseCombineMode combineMode = default)
        {
            this.Seed = seed;
            this.Scale = scale;

            this.Octaves = octaves;
            this.Persistance = persistance;
            this.Lacunarity = lacunarity;
            this.CombineMode = combineMode;

            this.Octaves = Math.Max(this.Octaves,       1);
            this.Lacunarity = Math.Max(this.Lacunarity, 1);
            this.Persistance = MathHelper.Clamp(this.Persistance, 0, 1);
        }

        public static PerlinNoiseSettings Parse(string text)
        {
            return new PerlinNoiseSettings(
                seed: ParseInt("seed"),
                scale: ParseDouble("scale"),
                octaves: ParseInt("octaves"),
                persistance: ParseDouble("persistance"),
                lacunarity: ParseDouble("lacunarity"));

            int ParseInt(string key)
            {
                var argValue = GetArg(key);
                try
                {
                    return int.Parse(argValue, CultureInfo.InvariantCulture);
                }
                catch
                {
                    throw new Exception($"Cannot parse value for \"{key}\"");
                }
            }

            double ParseDouble(string key)
            {
                var argValue = GetArg(key);
                try
                {
                    return double.Parse(argValue, CultureInfo.InvariantCulture);
                }
                catch
                {
                    throw new Exception($"Cannot parse value for \"{key}\"");
                }
            }

            string GetArg(string key)
            {
                try
                {
                    var indexOfKey = text.IndexOf(key);
                    var indexOfValueStart = text.IndexOf(':', indexOfKey + key.Length) + 1;

                    var indexOfValueEnd = text.IndexOfAny(ParsingValueEndChar, indexOfValueStart);
                    var valueText = text.Substring(indexOfValueStart, indexOfValueEnd - indexOfValueStart)
                                        .Trim(' ');
                    return valueText;
                }
                catch
                {
                    throw new Exception($"Cannot find value for \"{key}\"");
                }
            }
        }

        public DelegateCombineNoise GetCombineFunction()
        {
            switch (this.CombineMode)
            {
                case NoiseCombineMode.Average:
                    return this.CombineModeAverage;

                case NoiseCombineMode.Add:
                    return this.CombineModeAdd;

                case NoiseCombineMode.Multiply:
                    return this.CombineModeMultiply;

                case NoiseCombineMode.Max:
                    return this.CombineModeMax;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetSettingsCode()
        {
            //var sb = new StringBuilder($"new {nameof(PerlinNoiseSettings)}(" + Environment.NewLine, capacity: 256);
            var sb = new StringBuilder(capacity: 256);
            var isFirstArg = true;

            AppendArg("seed",        this.Seed.ToString());
            AppendArg("scale",       this.Scale.ToString(CultureInfo.InvariantCulture));
            AppendArg("octaves",     this.Octaves.ToString(CultureInfo.InvariantCulture));
            AppendArg("persistance", this.Persistance.ToString(CultureInfo.InvariantCulture));
            AppendArg("lacunarity",  this.Lacunarity.ToString(CultureInfo.InvariantCulture));

            void AppendArg(string argName, string value)
            {
                if (isFirstArg)
                {
                    isFirstArg = false;
                }
                else
                {
                    sb.AppendLine(",");
                }

                sb.Append(argName)
                  .Append(": ")
                  .Append(value);
            }

            //sb.Append(")");
            return sb.ToString();
        }

        private double CombineModeAdd(double current, double value)
        {
            return MathHelper.Clamp(current + value, 0, 1);
        }

        private double CombineModeAverage(double current, double value)
        {
            return (current + value) / 2;
        }

        private double CombineModeMax(double current, double value)
        {
            return Math.Max(current, value);
        }

        private double CombineModeMultiply(double current, double value)
        {
            return current * value;
        }
    }
}