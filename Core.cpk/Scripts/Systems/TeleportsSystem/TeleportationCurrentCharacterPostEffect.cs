namespace AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;

    public class TeleportationCurrentCharacterPostEffect : BasePostEffect
    {
        private static readonly EffectResource EffectResource
            = new("PostEffects/TeleportationCurrentCharacter");

        private EffectInstance effectInstance;

        private double intensity = 1;

        public override PostEffectsOrder DefaultOrder => PostEffectsOrder.Devices;

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
                .Set("TextureScreenBuffer", source);
            Rendering.GraphicsDevice.DrawFullScreen(this.effectInstance, BlendMode.Opaque);
        }

        protected override void OnDisable()
        {
            this.effectInstance.Dispose();
            this.effectInstance = null;
        }

        protected override void OnEnable()
        {
            this.effectInstance = EffectInstance.Create(EffectResource);
            this.SetEffectParameterIntensity();
        }

        private void SetEffectParameterIntensity()
        {
            this.effectInstance?
                .Parameters
                .Set("Intensity", (float)this.Intensity);
        }
    }
}