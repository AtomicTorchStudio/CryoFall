namespace AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EffectDeconstructionOutline : RenderingEffect
    {
        private static readonly IRenderingClientService Rendering = Api.Client.Rendering;

        private readonly IGraphicsDevice device;

        private readonly EffectInstance effect;

        private IComponentSpriteRenderer spriteRendererOutline;

        private IRenderTarget2D spriteRendererOutlineTexture;

        public EffectDeconstructionOutline()
        {
            this.device = Rendering.GraphicsDevice;
            this.effect = EffectInstance.Create(new EffectResource("Special/DeconstructionOutline"));
        }

        public override bool IsReady => this.effect.IsReady;

        public void Destroy()
        {
            this.spriteRendererOutlineTexture?.Dispose();
            this.spriteRendererOutlineTexture = null;
        }

        public override void Draw(
            IRenderTarget2D source,
            IRenderTarget2D destination,
            IGraphicsDevice graphicsDevice)
        {
            // setup and draw the outline into a separate render texture that will be later drawn as an overlay
            this.effect.Parameters.Set("OutlineSize",
                                       (float)(0.025
                                               * 256.0
                                               * Rendering.WorldCameraCurrentZoom
                                               / Math.Max(destination.Width, destination.Height)));

            if (this.spriteRendererOutlineTexture is null
                || this.spriteRendererOutlineTexture.Width != destination.Width
                || this.spriteRendererOutlineTexture.Height != destination.Height)
            {
                this.spriteRendererOutlineTexture?.Dispose();
                this.spriteRendererOutlineTexture
                    = Rendering.CreateRenderTexture(nameof(EffectDeconstructionOutline),
                                                    destination.Width,
                                                    destination.Height);
                this.spriteRendererOutline.TextureResource = this.spriteRendererOutlineTexture;

                var scale = 1 / (Rendering.WorldCameraCurrentZoom * Rendering.MainComposerViewportScale);
                this.spriteRendererOutline.Scale = scale;
            }

            this.device.SetRenderTarget(this.spriteRendererOutlineTexture);
            this.device.Clear(Color.FromArgb(0, 0, 0, 0));

            graphicsDevice.DrawTexture(
                source,
                source.Width,
                source.Height,
                effectInstance: this.effect,
                blendState: BlendMode.AlphaBlendPremultiplied);

            // blit the original sprite as is
            this.device.SetRenderTarget(destination);
            this.device.Clear(Color.FromArgb(0, 0, 0, 0));

            graphicsDevice.DrawTexture(
                source,
                source.Width,
                source.Height,
                effectInstance: null,
                blendState: BlendMode.AlphaBlendPremultiplied);
        }

        public override Vector2Int GetTargetTextureSize(IRenderTarget2D source)
        {
            return (source.Width, source.Height);
        }

        public void Setup(IComponentSpriteRenderer spriteRendererOutline)
        {
            this.spriteRendererOutline = spriteRendererOutline;
        }
    }
}