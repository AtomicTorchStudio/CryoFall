namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientConstructionGridRendererHelper
    {
        private static readonly RenderingMaterial RenderingMaterial
            = RenderingMaterial.Create(
                new EffectResource("ConstructionGrid"));

        private static readonly ITextureResource TextureResourceConstructionGrid
            = new TextureResource("FX/ConstructionGrid",
                                  qualityOffset: -100);

        public static void Setup(
            IClientSceneObject sceneObjectForComponents,
            IProtoStaticWorldObject protoStaticWorldObject)
        {
            // create construction grid renderer
            var layout = protoStaticWorldObject.Layout;
            var bounds = layout.Bounds;
            if (bounds.Size == Vector2Int.One)
            {
                CreateGridRenderer().PositionOffset = (0.5, 0.5);
            }
            else
            {
                var b = new BoundsDouble(
                    bounds.MinX + 0.5,
                    bounds.MinY + 0.5,
                    bounds.MaxX - 0.5,
                    bounds.MaxY - 0.5);

                CreateGridRenderer().PositionOffset = (b.MinX, b.MinY);
                CreateGridRenderer().PositionOffset = (b.MinX, b.MaxY);
                CreateGridRenderer().PositionOffset = (b.MaxX, b.MinY);
                CreateGridRenderer().PositionOffset = (b.MaxX, b.MaxY);
            }

            IComponentSpriteRenderer CreateGridRenderer()
            {
                var gridRenderer = Api.Client.Rendering.CreateSpriteRenderer(
                    sceneObjectForComponents,
                    TextureResourceConstructionGrid,
                    DrawOrder.Shadow + 2);

                gridRenderer.SpritePivotPoint = (0.5, 0.5);
                gridRenderer.PositionOffset = (0.5, 0.5);
                gridRenderer.RenderingMaterial = RenderingMaterial;
                return gridRenderer;
            }
        }
    }
}