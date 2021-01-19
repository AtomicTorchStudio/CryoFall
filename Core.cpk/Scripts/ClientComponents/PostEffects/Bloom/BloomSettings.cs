namespace AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.Bloom
{
    /// <summary>
    /// Based on http://www.alienscribbleinteractive.com/Tutorials/bloom_tutorial.html
    /// which is based on the original XNA version.
    /// </summary>
    public class BloomSettings
    {
        public static BloomSettings[] Presets =
        {
            //                Name           Thresh Blur Bloom Base BloomSat BaseSat
            new("Default",     0.25f, 4, 1.25f, 1,    1, 1),
            new("Soft",        0,     3, 1,     1,    1, 1),
            new("Desaturated", 0.5f,  8, 2,     1,    0, 1),
            new("Saturated",   0.25f, 4, 2,     1,    2, 0),
            new("Blurry",      0,     2, 1,     0.1f, 1, 1),
            new("Subtle",      0.5f,  2, 1,     1,    1, 1),
        };

        public readonly float BaseIntensity;

        public readonly float BaseSaturation;

        // Controls the amount of the bloom and base images that will be mixed into the final scene. Range 0 to 1.
        public readonly float BloomIntensity;

        // Independently control the color saturation of the bloom and base images. Zero is totally desaturated, 1.0 leaves saturation
        // unchanged, while higher values increase the saturation level.
        public readonly float BloomSaturation;

        // Controls how bright a pixel needs to be before it will bloom. Zero makes everything bloom equally, while higher values select
        // only brighter colors. Somewhere between 0.25 and 0.5 is good.
        public readonly float BloomThreshold;

        // Controls how much blurring is applied to the bloom image. The typical range is from 1 up to 10 or so.
        public readonly float BlurAmount;

        public readonly string Name;

        public BloomSettings(
            string name,
            float bloomThreshold,
            float blurAmount,
            float bloomIntensity,
            float baseIntensity,
            float bloomSaturation,
            float baseSaturation)
        {
            this.Name = name;
            this.BloomThreshold = bloomThreshold;
            this.BlurAmount = blurAmount;
            this.BloomIntensity = bloomIntensity;
            this.BaseIntensity = baseIntensity;
            this.BloomSaturation = bloomSaturation;
            this.BaseSaturation = baseSaturation;
        }
    }
}