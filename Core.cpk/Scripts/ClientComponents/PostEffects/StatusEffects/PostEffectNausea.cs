namespace AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.StatusEffects
{
    using System;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;

    /// <summary>
    /// Nausea post effect.
    /// </summary>
    public class PostEffectNausea : BasePostEffect
    {
        private static readonly EffectResource EffectResource
            = new EffectResource("PostEffects/StatusEffects/Nausea");

        private IGraphicsDevice device;

        private EffectInstance effectInstance;

        private double intensity;

        public override PostEffectsOrder DefaultOrder => PostEffectsOrder.StatusEffects;

        public double Intensity
        {
            get => this.intensity;
            set
            {
                if (this.intensity == value)
                {
                    return;
                }

                this.intensity = value;
                this.SetEffectParameters();
            }
        }

        public override bool IsCanRender
            => this.Intensity > 0
               && this.effectInstance.IsReady;

        public override void Render(IRenderTarget2D source, IRenderTarget2D destination)
        {
            this.device.SetRenderTarget(destination);
            this.device.DrawTexture(
                source,
                source.Width,
                source.Height,
                this.effectInstance,
                BlendMode.Opaque);
        }

        protected override void OnDisable()
        {
            this.effectInstance.Dispose();
            this.effectInstance = null;
        }

        protected override void OnEnable()
        {
            this.device = Rendering.GraphicsDevice;
            this.effectInstance = EffectInstance.Create(EffectResource);
            this.SetEffectParameters();
        }

        private void SetEffectParameters()
        {
            var value = this.intensity;
            // we want to make this effect noticeable even if the intensity is pretty low
            value = Math.Pow(value, 0.5);

            this.effectInstance?
                .Parameters
                .Set("Intensity", (float)value);
        }
    }
}