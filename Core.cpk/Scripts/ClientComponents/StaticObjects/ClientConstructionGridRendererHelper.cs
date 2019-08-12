namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;

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
            // let's create a construction grid renderer
            var layout = protoStaticWorldObject.Layout;
            var bounds = layout.Bounds.ToRectangleInt();

            // make construction grid size a 3 tiles larger on each side
            // than the construction blueprint object size
            bounds = bounds.Inflate(3, 3);

            var renderer = Api.Client.Rendering.CreateSpriteRenderer(
                sceneObjectForComponents,
                TextureResourceConstructionGrid,
                DrawOrder.Shadow + 2);

            renderer.SpritePivotPoint = (0, 0);
            renderer.SortByWorldPosition = false;
            renderer.BlendMode = BlendMode.Screen;
            renderer.RenderingMaterial = RenderingMaterial;
            renderer.PositionOffset = (bounds.X, bounds.Y);
            renderer.Scale = (bounds.Width, bounds.Height);
        }
    }
}