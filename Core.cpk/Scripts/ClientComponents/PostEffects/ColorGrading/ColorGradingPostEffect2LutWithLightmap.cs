namespace AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.ColorGrading
{
    using System;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;

    /// <summary>
    /// Color grading (blend between two LUT, apply to only non-lighted areas).
    /// </summary>
    public class ColorGradingPostEffect2LutWithLightmap : BasePostEffect
    {
        private static readonly EffectResource EffectResource
            = new EffectResource("PostEffects/ColorGrading2LutWithLightmap");

        private double blendFactor;

        private EffectInstance effectInstance;

        private Task<ITextureForImmediateRendering3D> taskLoadTextureLut1;

        private Task<ITextureForImmediateRendering3D> taskLoadTextureLut2;

        private TextureResource3D textureResourceLut1;

        private TextureResource3D textureResourceLut2;

        public double BlendFactor
        {
            get => this.blendFactor;
            set
            {
                if (this.blendFactor == value)
                {
                    return;
                }

                if (value < 0
                    || value > 1)
                {
                    throw new Exception("Intensity must be in range [0;1]");
                }

                this.blendFactor = value;
                this.SetEffectParameterBlendFactor();
            }
        }

        public override PostEffectsOrder DefaultOrder => PostEffectsOrder.ColorGrading;

        public override bool IsCanRender
            => this.effectInstance?.IsReady ?? false;

        public override void Render(IRenderTarget2D source, IRenderTarget2D destination)
        {
            this.effectInstance.Parameters
                .Set("TextureScreenBuffer", source)
                .Set("LightmapTexture",     ClientComponentLightingRenderer.RenderTargetLightMap);

            Rendering.GraphicsDevice.SetRenderTarget(destination);
            Rendering.GraphicsDevice.DrawFullScreen(this.effectInstance, BlendMode.Opaque);
        }

        public void Setup(
            TextureResource3D textureResourceLut1,
            TextureResource3D textureResourceLut2,
            double blendFactor)
        {
            this.BlendFactor = blendFactor;

            if (this.textureResourceLut1 == textureResourceLut1
                && this.textureResourceLut2 == textureResourceLut2)
            {
                return;
            }

            this.textureResourceLut1 = textureResourceLut1;
            this.textureResourceLut2 = textureResourceLut2;

            this.taskLoadTextureLut1 = Rendering.LoadTexture3D(textureResourceLut1);
            this.taskLoadTextureLut2 = Rendering.LoadTexture3D(textureResourceLut2);

            this.LoadTextures(this.taskLoadTextureLut1, this.taskLoadTextureLut2);
        }

        protected override void OnDisable()
        {
            this.effectInstance.Dispose();
            this.effectInstance = null;
        }

        protected override void OnEnable()
        {
            this.effectInstance = EffectInstance.Create(EffectResource);
            this.SetEffectParametersTextures();
            this.SetEffectParameterBlendFactor();
        }

        private async void LoadTextures(
            Task<ITextureForImmediateRendering3D> task1,
            Task<ITextureForImmediateRendering3D> task2)
        {
            await task1;
            await task2;

            if (this.taskLoadTextureLut1 == task1
                && this.taskLoadTextureLut2 == task2)
            {
                this.SetEffectParametersTextures();
            }
        }

        private void SetEffectParameterBlendFactor()
        {
            this.effectInstance?
                .Parameters
                .Set("BlendFactor", (float)this.BlendFactor);
        }

        private void SetEffectParametersTextures()
        {
            if (this.effectInstance is null)
            {
                return;
            }

            if (this.taskLoadTextureLut1?.IsCompleted ?? false)
            {
                this.effectInstance
                    .Parameters
                    .Set("TextureLut1", this.taskLoadTextureLut1.Result);
            }

            if (this.taskLoadTextureLut2?.IsCompleted ?? false)
            {
                this.effectInstance
                    .Parameters
                    .Set("TextureLut2", this.taskLoadTextureLut2.Result);
            }
        }
    }
}