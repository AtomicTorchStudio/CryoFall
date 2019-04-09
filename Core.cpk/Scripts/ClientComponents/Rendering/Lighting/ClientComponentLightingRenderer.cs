namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentLightingRenderer : ClientComponent
    {
        private const float MaxAdditiveLightFraction = 0.3f;

        private const float MinAdditiveLightFraction = 0.075f;

        private static readonly EffectResource EffectResourceLightingCompose
            = new EffectResource("Lighting/Compose");

        private static readonly IRenderingClientService Rendering = Api.Client.Rendering;

        private static float ambientLightFraction;

        private static ClientComponentLightingRenderer instance;

        private static IRenderTarget2D renderTargetLightMap;

        private ICamera2D camera;

        private EffectInstance effectInstanceCompose;

        /// <summary>
        /// This component holds a temporary render texture.
        /// </summary>
        private IComponentLayerRenderer layerRenderer;

        public static double AdditionalAmbientLight { get; set; }

        public static double AdditionalAmbightLightAdditiveFraction { get; set; }

        /// <summary>
        /// Ambient light fraction (value from 0 to 1).
        /// Value 0 means total darkness in non-lighted areas.
        /// It's supposed to be provided by the DayNightSystem.
        /// </summary>
        public static float AmbientLightFraction
        {
            get => ambientLightFraction;
            set
            {
                if (value == ambientLightFraction)
                {
                    return;
                }

                if (value < 0
                    || value > 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(AmbientLightFraction),
                        "Ambient value must in [0;1] range");
                }

                ambientLightFraction = value;
            }
        }

        public static ITextureResource RenderTargetLightMap => renderTargetLightMap;

        protected override void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }

            this.camera = null;

            this.layerRenderer.CustomDraw -= this.LayerRendererBeforeDrawHandler;
            this.layerRenderer.Destroy();
            this.layerRenderer = null;

            this.effectInstanceCompose.Dispose();
            this.effectInstanceCompose = null;

            renderTargetLightMap?.Dispose();
            renderTargetLightMap = null;
        }

        protected override void OnEnable()
        {
            instance = this;
            this.camera = Rendering.CreateCameraWorld(this.SceneObject, ClientLighting.RenderingTag, -1000);
            this.camera.ClearColor = Color.FromArgb(0x00, 0x00, 0x00, 0x00); // transparent black
            this.camera.DrawMode = CameraDrawMode.Manual;

            this.layerRenderer = Client.Rendering.CreateLayerRenderer(
                this.SceneObject,
                TextureResource.NoTexture,
                drawOrder: ClientLighting.IsDisplayMask
                               //  display as overlay (over everything)
                               ? DrawOrder.Overlay
                               : DrawOrder.Light);

            this.layerRenderer.CustomDraw += this.LayerRendererBeforeDrawHandler;

            this.effectInstanceCompose = EffectInstance.Create(EffectResourceLightingCompose);
        }

        private static Vector2Ushort GetRenderTargetSize()
        {
            var size = Rendering.ViewportSize;
            // it's ok to decrease the rendering size of lighting texture to 1/6th of the viewport
            // after all it's just a texture of relatively big and smooth "blobs"
            return new Vector2Ushort((ushort)(size.X / 6),
                                     (ushort)(size.Y / 6));
        }

        private void LayerRendererBeforeDrawHandler(IGraphicsDevice graphicsDevice)
        {
            if (!ClientPostEffectsManager.IsPostEffectsEnabled)
            {
                // post-processing is disabled (we consider lighting is also a kind of post-processing)
                return;
            }

            if (!this.effectInstanceCompose.IsReady)
            {
                // shaders are not ready
                return;
            }

            var renderTargetSize = GetRenderTargetSize();

            if (renderTargetLightMap == null
                || renderTargetLightMap.Width != renderTargetSize.X
                || renderTargetLightMap.Height != renderTargetSize.Y)
            {
                renderTargetLightMap = Rendering.CreateRenderTexture(
                    "Lightmap",
                    renderTargetSize.X,
                    renderTargetSize.Y,
                    format: SurfaceFormat.Color);
            }

            this.camera.RenderTarget = renderTargetLightMap;
            this.camera.DrawImmediate();
            this.camera.RenderTarget = null;

            // no we will render to the frame buffer
            graphicsDevice.SetRenderTarget(null);

            if (ClientLighting.IsDisplayMask)
            {
                // draw mask without blur into default frame buffer
                graphicsDevice.Blit(renderTargetLightMap, blendState: BlendMode.Opaque);
                return;
            }

            this.effectInstanceCompose.Parameters
                .Set("LightmapTexture", renderTargetLightMap)
                .Set("Ambient",
                     (float)(AmbientLightFraction + AdditionalAmbientLight))
                .Set("MinAdditiveLightFraction",
                     (float)(MinAdditiveLightFraction + AdditionalAmbightLightAdditiveFraction))
                .Set("MaxAdditiveLightFraction",
                     (float)(MaxAdditiveLightFraction + AdditionalAmbightLightAdditiveFraction))
                .Set("BaseTexture",
                     // yes, we will render the frame buffer by using the frame buffer
                     // a copy of the frame buffer will be made automatically
                     graphicsDevice.ComposerFramebuffer);

            graphicsDevice.DrawFullScreen(
                effectInstance: this.effectInstanceCompose,
                blendState: BlendMode.Opaque);
        }
    }
}