namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting
{
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;

    public class ClientComponentSpriteLightSource : BaseClientComponentLightSource
    {
        public static readonly TextureResource DefaultLightSpotTextureResource
            = new("FX/Light",
                  isTransparent: false,
                  isLinearSpace: true,
                  // always use max quality sprite, mipmap will do the job
                  qualityOffset: -100);

        private static readonly EffectResource EffectResourceLightingMaskDrawEffect
            = new("Lighting/LightSourceDrawEffect");

        private static readonly RenderingMaterial DefaultSpriteRenderingMaterial =
            RenderingMaterial.Create(EffectResourceLightingMaskDrawEffect);

        private IComponentSpriteRenderer renderer;

        private ITextureResource spotTextureResource = DefaultLightSpotTextureResource;

        public ITextureResource SpotTextureResource
        {
            get => this.spotTextureResource;
            set
            {
                if (Equals(this.spotTextureResource, value))
                {
                    return;
                }

                this.spotTextureResource = value;
                this.RefreshSpotTexture();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.renderer.Destroy();
            this.renderer = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.renderer = Rendering.CreateSpriteRenderer(
                this.SceneObject,
                TextureResource.NoTexture,
                drawOrder: DrawOrder.Light,
                renderingTag: DefaultLightRenderingTag);

            this.renderer.RenderingMaterial = DefaultSpriteRenderingMaterial;
            this.renderer.BlendMode = BlendMode.Screen;

            this.RefreshSpotTexture();

            this.LateUpdate(0);
        }

        protected override void SetProperties()
        {
            this.renderer.Color = this.LightColorPremultipliedAndWithOpacity;
            this.renderer.Scale = this.RenderingSize * 0.5; // multiply on 0.5 because of the support of HiDPI screen
            this.renderer.SpritePivotPoint = this.SpritePivotPoint;
            this.renderer.PositionOffset = this.PositionOffset;
        }

        private void RefreshSpotTexture()
        {
            this.renderer.TextureResource = this.spotTextureResource;
        }
    }
}