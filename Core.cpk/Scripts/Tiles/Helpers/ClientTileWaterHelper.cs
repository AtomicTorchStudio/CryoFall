namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ClientTileWaterHelper : ClientComponent
    {
        private static ClientTileWaterHelper instance;

        public static ClientTileWaterHelper Instance => instance ??= CreateInstance();

        public static void CreateWaterRenderer(IClientSceneObject sceneObject, IProtoTileWater protoTile)
        {
            Instance.CreateWaterRendererInternal(sceneObject, protoTile);
        }

        public static void CreateWaterRendererBlend(
            IClientSceneObject sceneObject,
            IProtoTileWater protoTile,
            ClientTileBlendHelper.BlendLayer blendLayer)
        {
            Instance.CreateWaterRendererBlendInternal(sceneObject, protoTile, blendLayer);
        }

        private static ClientTileWaterHelper CreateInstance()
        {
            return Client.Scene
                         .CreateSceneObject("Water helper")
                         .AddComponent<ClientTileWaterHelper>();
        }

        private void CreateWaterRendererBlendInternal(
            IClientSceneObject sceneObject,
            IProtoTileWater protoTileWater,
            ClientTileBlendHelper.BlendLayer blendLayer)
        {
            var renderer = Client.Rendering.CreateSpriteRenderer(
                sceneObject,
                protoTileWater.UnderwaterGroundTextureAtlas,
                drawOrder: DrawOrder.GroundBlend);
            renderer.RenderingMaterial = protoTileWater.ClientGetWaterBlendMaterial(blendLayer);
            renderer.SortByWorldPosition = false;
            renderer.IgnoreTextureQualityScaling = true;
            renderer.Size = ScriptingConstants.TileSizeRenderingVirtualSize;
        }

        private void CreateWaterRendererInternal(
            IClientSceneObject sceneObject,
            IProtoTileWater protoTileWater)
        {
            var renderer = Client.Rendering.CreateSpriteRenderer(
                sceneObject,
                protoTileWater.UnderwaterGroundTextureAtlas,
                // currently we place water instead of ground for tile
                drawOrder: DrawOrder.Ground);
            renderer.RenderingMaterial = protoTileWater.ClientGetWaterPrimaryMaterial();
            renderer.SortByWorldPosition = false;
            renderer.IgnoreTextureQualityScaling = true;
            renderer.Size = ScriptingConstants.TileSizeRenderingVirtualSize;
        }
    }
}