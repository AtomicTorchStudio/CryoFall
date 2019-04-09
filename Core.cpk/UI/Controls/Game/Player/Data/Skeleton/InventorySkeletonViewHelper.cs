namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;

    public static class InventorySkeletonViewHelper
    {
        public static readonly IClientApi Client = Api.Client;

        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
        private static readonly string renderingTag = "Inventory skeleton camera";

        public static ViewModelInventorySkeletonViewData Create(
            ICharacter character,
            ushort textureWidth,
            ushort textureHeight)
        {
            var sceneObjectCamera = Client.Scene.CreateSceneObject("Inventory skeleton camera");
            var camera = Client.Rendering.CreateCamera(sceneObjectCamera,
                                                       renderingTag: renderingTag,
                                                       drawOrder: -10,
                                                       drawMode: CameraDrawMode.Auto);

            var renderTarget = Client.Rendering.CreateRenderTexture(renderingTag, textureWidth, textureHeight);
            camera.RenderTarget = renderTarget;
            camera.ClearColor = Color.FromArgb(0, 0, 0, 0);
            camera.SetOrthographicProjection(textureWidth, textureHeight);

            var sceneObjectSkeleton = Client.Scene.CreateSceneObject("Inventory skeleton renderer");

            return new ViewModelInventorySkeletonViewData(
                character,
                sceneObjectCamera,
                sceneObjectSkeleton,
                renderTarget,
                renderingTag,
                textureWidth,
                textureHeight);
        }
    }
}