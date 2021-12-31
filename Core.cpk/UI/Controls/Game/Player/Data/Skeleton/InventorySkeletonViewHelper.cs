namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class InventorySkeletonViewHelper
    {
        public static readonly IClientApi Client = Api.Client;

        public static ViewModelInventorySkeletonViewData Create(
            ICharacter character,
            ushort textureWidth,
            ushort textureHeight)
        {
            var renderingTag = "Inventory skeleton - " + AtomicGuid.NewGuid();
            var sceneObjectCamera = Client.Scene.CreateSceneObject(renderingTag + " - camera");
            var camera = Client.Rendering.CreateCamera(sceneObjectCamera,
                                                       renderingTag: renderingTag,
                                                       drawOrder: -10,
                                                       drawMode: CameraDrawMode.Auto);

            var renderTarget = Client.Rendering.CreateRenderTexture(renderingTag, textureWidth, textureHeight);
            camera.RenderTarget = renderTarget;
            camera.ClearColor = Color.FromArgb(0, 0, 0, 0);
            camera.SetOrthographicProjection(textureWidth, textureHeight);

            var sceneObjectSkeleton = Client.Scene.CreateSceneObject(renderingTag + " - renderer");

            return new ViewModelInventorySkeletonViewData(
                character,
                sceneObjectCamera,
                sceneObjectSkeleton,
                camera,
                renderTarget,
                renderingTag,
                textureWidth,
                textureHeight);
        }
    }
}