namespace AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.Bloom
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.Blur;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// Based on http://www.alienscribbleinteractive.com/Tutorials/bloom_tutorial.html
    /// which is based on the original XNA version.
    /// </summary>
    public class BloomPostEffect : BasePostEffect
    {
        // You can select other modes in order to debug rendering.
        private const IntermediateBuffer DisplayedBuffer = IntermediateBuffer.FinalResult;

        private static readonly EffectResource EffectResourceBloomCombine
            = new EffectResource("PostEffects/BloomCombine");

        private static readonly EffectResource EffectResourceBloomExtract
            = new EffectResource("PostEffects/BloomExtract");

        private static readonly EffectResource EffectResourceGaussianBlur
            = new EffectResource("PostEffects/GaussianBlur");

        private IGraphicsDevice device;

        private EffectInstance effectBloomCombine;

        private EffectInstance effectBloomExtract;

        private EffectInstance effectBlurHorizontal;

        private EffectInstance effectBlurVertical;

        private bool isBlurParametersDirty = true;

        private IRenderTarget2D lastBloomCombineSource;

        private Vector2Int lastTextureSize;

        private BloomSettings settings;

        public enum IntermediateBuffer
        {
            PreBloom,

            BlurredHorizontally,

            BlurredBothWays,

            FinalResult,
        }

        public override bool IsCanRender
            => this.effectBloomExtract.IsReady
               && this.effectBloomCombine.IsReady
               && this.effectBlurHorizontal.IsReady
               && this.effectBlurVertical.IsReady;

        public override void Render(IRenderTarget2D source, IRenderTarget2D destination)
        {
            // The render targets will be half the size of the viewport in order to minimize fillrate.
            // It doesn't affect the quality as we will apply blur.
            // TODO: it seems we can decrease it further more.
            var size = Rendering.ViewportSize;
            size = new Vector2Ushort((ushort)(size.X / 2),
                                     (ushort)(size.Y / 2));

            if (this.lastTextureSize != size)
            {
                this.lastTextureSize = size;
                // this must be reset in order to reassign source texture to effect
                this.lastBloomCombineSource = null;
                this.isBlurParametersDirty = true;
            }

            using var renderTarget1 = Rendering.GetTempRenderTexture(size.X, size.Y);
            using var renderTarget2 = Rendering.GetTempRenderTexture(size.X, size.Y);
            if (this.isBlurParametersDirty)
            {
                this.SetBlurParameters(size);
            }

            // clear render target 1
            this.device.SetRenderTarget(renderTarget1);
            this.device.Clear(Color.FromArgb(0, 0, 0, 0));

            // Pass 1: draw the scene into rendertarget 1,
            // using a shader that extracts only the brightest parts of the image.
            this.DrawFullscreenQuad(
                source,
                renderTarget1.Width,
                renderTarget1.Height,
                this.effectBloomExtract,
                IntermediateBuffer.PreBloom,
                blendState: BlendMode.Opaque);

            // Pass 2: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.
            this.device.SetRenderTarget(renderTarget2);
            this.DrawFullscreenQuad(
                renderTarget1,
                renderTarget2.Width,
                renderTarget2.Height,
                this.effectBlurHorizontal,
                IntermediateBuffer.BlurredHorizontally);

            // Pass 3: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            this.device.SetRenderTarget(renderTarget1);
            this.DrawFullscreenQuad(
                renderTarget2,
                renderTarget1.Width,
                renderTarget1.Height,
                this.effectBlurVertical,
                IntermediateBuffer.BlurredBothWays);

            // Pass 4: draw both rendertarget 1 and the original scene image into the destination,
            // using a shader that combines them to produce the final bloomed result.
            this.device.SetRenderTarget(destination);

            if (this.lastBloomCombineSource != source)
            {
                this.lastBloomCombineSource = source;
                this.effectBloomCombine.Parameters.Set("BaseTexture", source);
            }

            var viewport = Rendering.ViewportSize;
            this.DrawFullscreenQuad(
                renderTarget1,
                viewport.X,
                viewport.Y,
                this.effectBloomCombine,
                IntermediateBuffer.FinalResult,
                blendState: BlendMode.Opaque);
        }

        public void Setup(BloomSettings newSettings)
        {
            this.settings = newSettings;
            this.effectBloomCombine.Parameters
                .Set("BloomIntensity",  this.settings.BloomIntensity)
                .Set("BaseIntensity",   this.settings.BaseIntensity)
                .Set("BloomSaturation", this.settings.BloomSaturation)
                .Set("BaseSaturation",  this.settings.BaseSaturation);

            this.effectBloomExtract.Parameters.Set("BloomThreshold", this.settings.BloomThreshold);
            this.isBlurParametersDirty = true;
        }

        protected override void OnDisable()
        {
            this.effectBloomCombine.Dispose();
            this.effectBloomExtract.Dispose();
            this.effectBlurHorizontal.Dispose();
            this.effectBlurVertical.Dispose();

            this.effectBloomCombine = null;
            this.effectBloomExtract = null;
            this.effectBlurHorizontal = null;
            this.effectBlurVertical = null;
        }

        protected override void OnEnable()
        {
            this.device = Rendering.GraphicsDevice;
            this.effectBloomCombine = EffectInstance.Create(EffectResourceBloomCombine);
            this.effectBloomExtract = EffectInstance.Create(EffectResourceBloomExtract);
            this.effectBlurHorizontal = EffectInstance.Create(EffectResourceGaussianBlur);
            this.effectBlurVertical = EffectInstance.Create(EffectResourceGaussianBlur);
        }

        private void DrawFullscreenQuad(
            IRenderTarget2D textureToDraw,
            int width,
            int height,
            EffectInstance effect,
            IntermediateBuffer currentBuffer,
            BlendMode blendState = BlendMode.AlphaBlendPremultiplied)
        {
            if (DisplayedBuffer < currentBuffer)
            {
                // The user has selected one of the show intermediate buffer options,
                // we still draw the quad to make sure the image will end up on the screen,
                // but might need to skip applying the custom pixel shader.
                effect = null;
            }

            // Clear with transparent black. Must do this for each target if using transparent target.
            this.device.Clear(Color.FromArgb(255, 0, 0, 0));
            this.device.DrawTexture(textureToDraw, width, height, effect, blendState);
        }

        private void SetBlurParameters(Vector2Int size)
        {
            var blurAmount = this.settings.BlurAmount;

            BlurPostEffect.SetBlurEffectParameters(
                this.effectBlurHorizontal,
                dx: 1.0f / size.X,
                dy: 0,
                blurAmount: blurAmount);

            BlurPostEffect.SetBlurEffectParameters(
                this.effectBlurVertical,
                dx: 0,
                dy: 1.0f / size.Y,
                blurAmount: blurAmount);

            this.isBlurParametersDirty = false;
        }
    }
}