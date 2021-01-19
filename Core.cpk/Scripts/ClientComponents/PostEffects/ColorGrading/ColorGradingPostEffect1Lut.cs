namespace AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.ColorGrading
{
    using System;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;

    /// <summary>
    /// Color grading (single LUT).
    /// </summary>
    public class ColorGradingPostEffect1Lut : BasePostEffect
    {
        private static readonly EffectResource EffectResource
            = new("PostEffects/ColorGrading1Lut");

        private EffectInstance effectInstance;

        private double intensity = 1;

        private Task<ITextureForImmediateRendering3D> taskLoadTextureLut;

        private TextureResource3D textureResourceLut;

        public override PostEffectsOrder DefaultOrder => PostEffectsOrder.ColorGrading;

        public double Intensity
        {
            get => this.intensity;
            set
            {
                if (this.intensity == value)
                {
                    return;
                }

                if (value < 0
                    || value > 1)
                {
                    throw new Exception("Intensity must be in range [0;1]");
                }

                this.intensity = value;
                this.SetEffectParameterIntensity();
            }
        }

        public override bool IsCanRender
            => this.intensity > 0
               && this.effectInstance.IsReady;

        public override void Render(IRenderTarget2D source, IRenderTarget2D destination)
        {
            Rendering.GraphicsDevice.SetRenderTarget(destination);
            this.effectInstance.Parameters.Set("TextureScreenBuffer", source);
            Rendering.GraphicsDevice.DrawFullScreen(this.effectInstance, BlendMode.Opaque);
        }

        public void Setup(TextureResource3D textureResourceLut, double intensity = 1)
        {
            this.Intensity = intensity;
            if (this.textureResourceLut == textureResourceLut)
            {
                // no need to change
                return;
            }

            this.textureResourceLut = textureResourceLut;
            this.taskLoadTextureLut = Rendering.LoadTexture3D(textureResourceLut);
            this.LoadTexture(this.taskLoadTextureLut);
        }

        protected override void OnDisable()
        {
            this.effectInstance.Dispose();
            this.effectInstance = null;
        }

        protected override void OnEnable()
        {
            this.effectInstance = EffectInstance.Create(EffectResource);
            this.SetEffectParameterTexture();
            this.SetEffectParameterIntensity();
        }

        private async void LoadTexture(Task<ITextureForImmediateRendering3D> task)
        {
            await task;
            if (this.taskLoadTextureLut == task)
            {
                this.SetEffectParameterTexture();
            }
        }

        private void SetEffectParameterIntensity()
        {
            this.effectInstance?
                .Parameters
                .Set("Intensity", (float)this.Intensity);
        }

        private void SetEffectParameterTexture()
        {
            if (this.taskLoadTextureLut?.IsCompleted ?? false)
            {
                this.effectInstance?
                    .Parameters
                    .Set("TextureLut", this.taskLoadTextureLut.Result);
            }
        }
    }
}