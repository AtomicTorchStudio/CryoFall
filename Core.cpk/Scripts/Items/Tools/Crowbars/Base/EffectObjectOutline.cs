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

    public class EffectObjectOutline : RenderingEffect
    {
        private static readonly IRenderingClientService Rendering = Api.Client.Rendering;

        private readonly IGraphicsDevice device;

        private readonly EffectInstance effect;

        private Color colorMultiply;

        private Color colorOutline;

        private Size2F? scale;

        private IComponentSpriteRenderer spriteRendererOutline;

        private IRenderTarget2D spriteRendererOutlineTexture;

        public EffectObjectOutline()
        {
            this.device = Rendering.GraphicsDevice;
            this.effect = EffectInstance.Create(new EffectResource("Special/ObjectOutline"));
        }

        public Color ColorMultiply
        {
            get => this.colorMultiply;
            set
            {
                if (this.colorMultiply == value)
                {
                    return;
                }

                this.colorMultiply = value;
                this.effect.Parameters.Set("ColorMultiply", this.colorMultiply);
            }
        }

        public Color ColorOutline
        {
            get => this.colorOutline;
            set
            {
                if (this.colorOutline == value)
                {
                    return;
                }

                this.colorOutline = value;
                this.effect.Parameters.Set("ColorOutline", this.colorOutline);
            }
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
            IGraphicsDevice graphicsDevice,
            Vector2D originalSize)
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
                    = Rendering.CreateRenderTexture(nameof(EffectObjectOutline),
                                                    destination.Width,
                                                    destination.Height);
                this.spriteRendererOutline.TextureResource = this.spriteRendererOutlineTexture;
            }

            this.spriteRendererOutline.Size = ((this.scale?.X ?? 1) * originalSize.X / 2,
                                               (this.scale?.Y ?? 1) * originalSize.Y / 2);

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

        public void Setup(IComponentSpriteRenderer spriteRendererOutline, Size2F? scale)
        {
            this.spriteRendererOutline = spriteRendererOutline;
            this.scale = scale;
        }
    }
}