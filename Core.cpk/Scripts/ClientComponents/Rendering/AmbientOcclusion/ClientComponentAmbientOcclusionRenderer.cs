namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.AmbientOcclusion
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.Blur;
    using AtomicTorch.CBND.CoreMod.ClientOptions.Video;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;

    public class ClientComponentAmbientOcclusionRenderer : ClientComponent
    {
        private static readonly EffectResource EffectResourceAmbientOcclusionCompose
            = new("AmbientOcclusion/Compose");

        private static readonly IRenderingClientService Rendering = Api.Client.Rendering;

        private static ClientComponentAmbientOcclusionRenderer instance;

        private BlurPostEffect blurPostEffect;

        private ICamera2D camera;

        private EffectInstance effectInstanceCompose;

        /// <summary>
        /// This component holds a temporary render texture.
        /// </summary>
        private IComponentLayerRenderer layerRenderer;

        protected override void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }

            this.camera = null;

            this.blurPostEffect.IsEnabled = false;

            this.layerRenderer.CustomDraw -= this.LayerRendererBeforeDrawHandler;
            this.layerRenderer.Destroy();
            this.layerRenderer = null;

            this.effectInstanceCompose.Dispose();
            this.effectInstanceCompose = null;
        }

        protected override void OnEnable()
        {
            instance = this;
            this.camera = Rendering.CreateCameraWorld(this.SceneObject, ClientAmbientOcclusion.RenderingTag, -1000);
            this.camera.ClearColor = ClientAmbientOcclusion.IsDisplayMask
                                         ? Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF)
                                         : Color.FromArgb(0x00, 0x00, 0x00, 0x00); // transparent black
            this.camera.DrawMode = CameraDrawMode.Manual;

            this.blurPostEffect = new BlurPostEffect
            {
                RenderTextureDownsampling = 1,
                Passes = 2,
                IsEnabled = true
            };

            this.layerRenderer = Client.Rendering.CreateLayerRenderer(
                this.SceneObject,
                TextureResource.NoTexture,
                drawOrder: ClientAmbientOcclusion.IsDisplayMask
                               //  display as overlay (over everything)
                               ? DrawOrder.Overlay
                               : DrawOrder.Occlusion);

            this.layerRenderer.CustomDraw += this.LayerRendererBeforeDrawHandler;

            this.effectInstanceCompose = EffectInstance.Create(EffectResourceAmbientOcclusionCompose);
        }

        private void LayerRendererBeforeDrawHandler(IGraphicsDevice graphicsDevice)
        {
            if (!ClientPostEffectsManager.IsPostEffectsEnabled)
            {
                // post-processing is disabled (we consider AO is also a kind of post-processing)
                return;
            }

            if (!VideoOptionAmbientOcclusion.IsEnabled)
            {
                // this effect is disabled in video options
                return;
            }

            if (!this.blurPostEffect.IsCanRender
                || !this.effectInstanceCompose.IsReady)
            {
                // shaders are not ready
                return;
            }

            var viewportSize = Rendering.ViewportSize;
            if (viewportSize == (1, 1))
            {
                // the game is minimized
                return;
            }
            
            var surfaceFormat = ClientAmbientOcclusion.IsDisplayMask
                                    ? SurfaceFormat.Color
                                    : SurfaceFormat.SingleColor;

            using var renderTarget1 = Rendering.GetTempRenderTexture(
                viewportSize.X,
                viewportSize.Y,
                surfaceFormat);
            this.camera.RenderTarget = renderTarget1;
            this.camera.DrawImmediate();
            this.camera.RenderTarget = null;

            if (ClientAmbientOcclusion.IsDisplayMask
                && !ClientAmbientOcclusion.IsDisplayMaskWithBlur)
            {
                // draw mask without blur into default frame buffer
                graphicsDevice.Blit(renderTarget1, blendState: BlendMode.AlphaBlendNonPremultiplied);
                return;
            }

            var zoomFactor = this.camera.Zoom;
            this.blurPostEffect.BlurAmountHorizontal = ClientAmbientOcclusion.BlurDistanceHorizontal * zoomFactor;
            this.blurPostEffect.BlurAmountVertical = ClientAmbientOcclusion.BlurDistanceVertical * zoomFactor;

            using var renderTarget2 = Rendering.GetTempRenderTexture(
                renderTarget1.Width,
                renderTarget1.Height,
                format: surfaceFormat);
            // blit renderTexture to tempDestination with Blur shader
            this.blurPostEffect.Render(renderTarget1, renderTarget2);

            // restore framebuffer render target and blit with ComposeShader
            graphicsDevice.SetRenderTarget(null);

            // draw mask with blur into default frame buffer
            graphicsDevice.Blit(
                renderTarget2,
                BlendMode.AlphaBlendNonPremultiplied,
                effectInstance: ClientAmbientOcclusion.IsDisplayMask ? null : this.effectInstanceCompose);
        }
    }
}