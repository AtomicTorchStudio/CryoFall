namespace AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.NightVision
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;

    /// <summary>
    /// Night vision post effect.
    /// </summary>
    public class NightVisionPostEffect : BasePostEffect
    {
        private EffectInstance effectInstance;

        private EffectResource effectResource;

        private double intensity = 1;

        public override PostEffectsOrder DefaultOrder => PostEffectsOrder.Devices;

        public EffectResource EffectResource
        {
            get => this.effectResource;
            set
            {
                if (this.effectResource == value)
                {
                    return;
                }

                this.effectResource = value;

                this.TryDestroyEffectInstance();
                this.TryCreateEffectInstance();
            }
        }

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
               && (this.effectInstance?.IsReady ?? false);

        public override void Render(IRenderTarget2D source, IRenderTarget2D destination)
        {
            Rendering.GraphicsDevice.SetRenderTarget(destination);
            this.effectInstance.Parameters
                .Set("TextureScreenBuffer", source)
                .Set("AdditionalLight",     (float)TimeOfDaySystem.DayFraction);
            Rendering.GraphicsDevice.DrawFullScreen(this.effectInstance, BlendMode.Opaque);
        }

        protected override void OnDisable()
        {
            this.TryDestroyEffectInstance();
        }

        protected override void OnEnable()
        {
            this.TryCreateEffectInstance();
        }

        private void SetEffectParameterIntensity()
        {
            this.effectInstance?
                .Parameters
                .Set("Intensity", (float)this.Intensity);
        }

        private void TryCreateEffectInstance()
        {
            if (this.effectResource is null
                || !this.IsEnabled)
            {
                return;
            }

            this.effectInstance = EffectInstance.Create(this.effectResource);
            this.SetEffectParameterIntensity();
        }

        private void TryDestroyEffectInstance()
        {
            this.effectInstance?.Dispose();
            this.effectInstance = null;
        }
    }
}