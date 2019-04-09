namespace AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.Blur
{
    using System;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// Based on http://www.alienscribbleinteractive.com/Tutorials/bloom_tutorial.html
    /// which is based on the original XNA version.
    /// </summary>
    public class BlurPostEffect : BasePostEffect
    {
        private static readonly EffectResource EffectResourceGaussianBlur
            = new EffectResource("PostEffects/GaussianBlur");

        private double blurAmountHorizontal = 4;

        private double blurAmountVertical = 4;

        private IGraphicsDevice device;

        private EffectInstance effectBlurHorizontal;

        private EffectInstance effectBlurVertical;

        private bool isDirty = true;

        private Vector2Int lastTextureSize;

        private byte passes = 1;

        private double renderTextureDownsampling = 1;

        public double BlurAmountHorizontal
        {
            get => this.blurAmountHorizontal;
            set
            {
                if (this.blurAmountHorizontal == value)
                {
                    return;
                }

                this.blurAmountHorizontal = value;
                this.isDirty = true;
            }
        }

        public double BlurAmountVertical
        {
            get => this.blurAmountVertical;
            set
            {
                if (this.blurAmountVertical == value)
                {
                    return;
                }

                this.blurAmountVertical = value;
                this.isDirty = true;
            }
        }

        public override bool IsCanRender
            => this.effectBlurHorizontal.IsReady
               && this.effectBlurVertical.IsReady;

        public byte Passes
        {
            get => this.passes;
            set
            {
                if (this.passes == value)
                {
                    return;
                }

                if (value == 0)
                {
                    throw new Exception("Passes count must be >=1");
                }

                this.passes = value;
            }
        }

        /// <summary>
        /// Downsampling coefficient for viewport.
        /// Default value is 1 (no downsampling).
        /// For render at half resolution, set 2 (it will produce 4 times less pixels).
        /// Must be a positive value >= 1.
        /// </summary>
        public double RenderTextureDownsampling
        {
            get => this.renderTextureDownsampling;
            set
            {
                if (this.renderTextureDownsampling == value)
                {
                    return;
                }

                if (value < 1)
                {
                    throw new Exception("Scale coefficient must be > 1");
                }

                this.renderTextureDownsampling = value;
                this.isDirty = true;
            }
        }

        public static void SetBlurEffectParameters(
            EffectInstance effectInstance,
            float dx,
            float dy,
            double blurAmount)
        {
            // TODO: Look up SAMPLE_COUNT in the effect to get how many samples the gaussian blur effect supports.
            const int sampleCount = 15;

            // Create temporary arrays for computing our filter settings.
            var sampleWeights = new float[sampleCount];
            var sampleOffsets = new Vector2F[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = Vector2F.Zero;

            // Maintain a sum of all the weighting values.
            var totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned along a line in both directions from the center.
            for (var i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                var weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one. This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by positioning us nicely in between two texels.
                var sampleOffset = i * 2 + 1.5f;

                var delta = new Vector2F(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = delta.Negate();
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (var i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            effectInstance.Parameters
                          .Set("SampleWeights", sampleWeights)
                          .Set("SampleOffsets", sampleOffsets);

            float ComputeGaussian(float n)
            {
                var theta = blurAmount;
                return (float)(1.0 / Math.Sqrt(2 * Math.PI * theta) * Math.Exp(-(n * n) / (2 * theta * theta)));
            }
        }

        public override void Render(IRenderTarget2D source, IRenderTarget2D destination)
        {
            var size = Rendering.ViewportSize;
            size = new Vector2Ushort(
                (ushort)Math.Round(size.X / this.renderTextureDownsampling),
                (ushort)Math.Round(size.Y / this.renderTextureDownsampling));

            if (this.lastTextureSize != size)
            {
                this.lastTextureSize = size;
                this.isDirty = true;
            }

            using (var renderTarget1 = Rendering.GetTempRenderTexture(size.X, size.Y))
            using (var renderTarget2 = Rendering.GetTempRenderTexture(size.X, size.Y))
            {
                if (this.isDirty)
                {
                    this.SetBlurEffectParameters();
                }

                for (var i = 0; i < this.passes; i++)
                {
                    // Pass 1: draw from source into rendertarget 1,
                    // using a shader to apply a horizontal gaussian blur filter.
                    this.device.SetRenderTarget(renderTarget1);
                    this.device.DrawTexture(
                        source,
                        renderTarget1.Width,
                        renderTarget1.Height,
                        this.effectBlurHorizontal,
                        BlendMode.Opaque);

                    // Pass 2: draw from rendertarget 1 into rendertarget 2,
                    // using a shader to apply a vertical gaussian blur filter.
                    this.device.SetRenderTarget(renderTarget2);
                    this.device.DrawTexture(
                        renderTarget1,
                        renderTarget2.Width,
                        renderTarget2.Height,
                        this.effectBlurVertical,
                        BlendMode.Opaque);

                    // for second and later passes use renderTarget2 as the source
                    source = renderTarget2;
                }

                // Pass 3: draw rendertarget 2 into the destination
                this.device.SetRenderTarget(destination);
                this.device.Blit(renderTarget2, blendState: BlendMode.Opaque);
            }
        }

        protected override void OnDisable()
        {
            this.effectBlurHorizontal.Dispose();
            this.effectBlurVertical.Dispose();
            this.effectBlurHorizontal = null;
            this.effectBlurVertical = null;
        }

        protected override void OnEnable()
        {
            this.device = Rendering.GraphicsDevice;
            this.effectBlurHorizontal = EffectInstance.Create(EffectResourceGaussianBlur);
            this.effectBlurVertical = EffectInstance.Create(EffectResourceGaussianBlur);
            this.isDirty = true;
        }

        private void SetBlurEffectParameters()
        {
            SetBlurEffectParameters(
                this.effectBlurHorizontal,
                dx: 1.0f / this.lastTextureSize.X,
                dy: 0,
                blurAmount: this.blurAmountHorizontal / this.renderTextureDownsampling);

            SetBlurEffectParameters(
                this.effectBlurVertical,
                dx: 0,
                dy: 1.0f / this.lastTextureSize.Y,
                blurAmount: this.blurAmountVertical / this.renderTextureDownsampling);

            this.isDirty = false;
        }
    }
}