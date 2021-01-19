namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Signs
{
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectSignPicture
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectSign
            <TPrivateState,
                TPublicState,
                TClientState>
        where TPrivateState : ObjectWithOwnerPrivateState, new()
        where TPublicState : ObjectSignPublicState, new()
        where TClientState : ObjectSignClientState, new()
    {
        public override string InteractionTooltipText => InteractionTooltipTexts.Configure;

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowSignPicture.Open(
                new ViewModelWindowSignPicture(data.GameObject));
        }

        protected override void ClientRefreshSignRendering(
            IComponentSpriteRenderer rendererSignContent,
            TPublicState publicState)
        {
            var texturePicture = SharedSignPictureHelper.GetTextureResource(publicState.Text);
            if (texturePicture is null)
            {
                rendererSignContent.TextureResource = null;
                rendererSignContent.IsEnabled = false;
                return;
            }

            rendererSignContent.TextureResource =
                new ProceduralTexture(
                    nameof(ProtoObjectSign) + ": " + publicState.Text,
                    isTransparent: true,
                    isUseCache: true,
                    generateTextureCallback:
                    request => this.ClientGenerateProceduralTextureForPicture(texturePicture, request),
                    dependsOn: new[] { (ITextureResource)texturePicture });
            rendererSignContent.Scale = Client.Rendering.SpriteQualitySizeMultiplier;
            rendererSignContent.IsEnabled = true;
        }

        private async Task<ITextureResource> ClientGenerateProceduralTextureForPicture(
            TextureResource pictureTextureResource,
            ProceduralTextureRequest request)
        {
            var rendering = Client.Rendering;
            var renderingTag = request.TextureName;

            var originalTextureSize = await rendering.GetTextureSize(pictureTextureResource);
            request.ThrowIfCancelled();

            var scale = 3;
            var scaledTextureSize = new Vector2Ushort((ushort)(originalTextureSize.X * scale),
                                                      (ushort)(originalTextureSize.Y * scale));

            var renderTexture = rendering.CreateRenderTexture(renderingTag,
                                                              scaledTextureSize.X,
                                                              scaledTextureSize.Y);
            var cameraObject = Client.Scene.CreateSceneObject(renderingTag);
            var camera = rendering.CreateCamera(cameraObject,
                                                renderingTag,
                                                drawOrder: -200);
            camera.ClearColor = Color.FromArgb(0, 0, 0, 0);
            camera.RenderTarget = renderTexture;
            camera.TextureFilter = TextureFilter.Point;
            camera.SetOrthographicProjection(scaledTextureSize.X, scaledTextureSize.Y);

            // draw sprite with the required scale
            rendering.CreateSpriteRenderer(
                cameraObject,
                pictureTextureResource,
                renderingTag: renderingTag,
                // draw down
                spritePivotPoint: (0, 1),
                scale: scale);

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(isTransparent: true);
            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }
    }

    public abstract class ProtoObjectSignPicture
        : ProtoObjectSignPicture<ObjectWithOwnerPrivateState, ObjectSignPublicState, ObjectSignClientState>
    {
    }
}