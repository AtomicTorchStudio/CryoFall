namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;

    public class BootstrapperSkinToneRemapping : BaseBootstrapper
    {
        //public override async void ClientInitialize()
        //{
        //    await CreateTestTexturePreviewer();
        //}

        private static async Task CreateTestTexturePreviewer()
        {
            var sourceTextureResource = new TextureResource("FX/SkinToneMaps/ImageToModify.png");
            var lutTextureResource = new TextureResource3D("FX/SkinToneMaps/Lut1.png", depth: 24, isTransparent: false);
            var lutTexture3D = await Client.Rendering.LoadTexture3D(lutTextureResource);

            var textureSize = await Client.Rendering.GetTextureSize(sourceTextureResource);
            var textureWidth = textureSize.X;
            var textureHeight = textureSize.Y;

            var renderingTag = nameof(BootstrapperSkinToneRemapping);
            var renderingMaterial = RenderingMaterial.Create(new EffectResource("ColorLutRemap"));
            renderingMaterial.EffectParameters.Set("TextureLut", lutTexture3D);

            var sceneObjectCamera = Client.Scene.CreateSceneObject("Skin tone remapping camera");
            var camera = Client.Rendering.CreateCamera(sceneObjectCamera,
                                                       renderingTag: renderingTag,
                                                       drawOrder: -10,
                                                       drawMode: CameraDrawMode.Auto);

            var renderTarget = Client.Rendering.CreateRenderTexture(renderingTag, textureWidth, textureHeight);
            camera.RenderTarget = renderTarget;
            camera.ClearColor = Color.FromArgb(0, 0, 0, 0);
            camera.SetOrthographicProjection(textureWidth, textureHeight);

            Client.Rendering.CreateSpriteRenderer(sceneObjectCamera,
                                                  sourceTextureResource,
                                                  renderingTag: renderingTag,
                                                  // draw down
                                                  spritePivotPoint: (0, 1))
                  .RenderingMaterial = renderingMaterial;

            var rectangle = new Rectangle
            {
                Width = textureWidth,
                Height = textureHeight,
                Fill = Api.Client.UI.CreateImageBrushForRenderTarget(camera.RenderTarget),
                Stretch = Stretch.Uniform,
                UseLayoutRounding = true
            };

            Panel.SetZIndex(rectangle, int.MaxValue);
            Api.Client.UI.LayoutRootChildren.Add(rectangle);
        }
    }
}