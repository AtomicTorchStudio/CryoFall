namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentTerrainDecalRenderer : ClientComponent
    {
        private static readonly IRenderingClientService RenderingService = Api.Client.Rendering;

        public ProtoTileDecal Decal;

        private IComponentSpriteRenderer decalRenderer;

        private Vector2Ushort position;

        public void Setup(ProtoTileDecal decal, Vector2Ushort position)
        {
            this.Decal = decal;
            this.position = position;
            this.Refresh();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.decalRenderer.Destroy();
            this.decalRenderer = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.Refresh();
        }

        private void Refresh()
        {
            if (this.Decal is null)
            {
                // decal is not setup (yet)
                return;
            }

            var decalTexture = this.Decal.GetTexture(this.position, out var drawMode);
            this.decalRenderer = RenderingService.CreateSpriteRenderer(
                this.SceneObject,
                decalTexture,
                drawOrder: this.Decal.DrawOrder);
            this.decalRenderer.SortByWorldPosition = false;
            this.decalRenderer.DrawMode = drawMode;
        }
    }
}