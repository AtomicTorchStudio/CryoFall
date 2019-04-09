namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Floors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BootstrapperTestFloorAtlas : BaseBootstrapper
    {
        //public override async void ClientInitialize()
        //{
        //	await CreateTestTexturePreviewer();
        //}

        private static async Task CreateTestTexturePreviewer()
        {
            var protoFloor = Api.GetProtoEntity<ObjectFloorWood>();
            var atlasTexture = protoFloor.DefaultTexture;
            var request = new ProceduralTextureRequest();

            var instance = FloorTextureComposer.Instance;
            var resultAtlasTexture = await instance.GenerateProceduralTextureNonAtlas(
                                         "Test",
                                         (TextureAtlasResource)atlasTexture,
                                         request);

            var rectangle = new Rectangle
            {
                Width = instance.AtlasTextureWidth,
                Height = instance.AtlasTextureHeight,
                //Fill = ClientApi.UI.CreateImageBrushForRenderTarget(camera.RenderTarget),
                Stretch = Stretch.Uniform,
                UseLayoutRounding = true,
            };

            Panel.SetZIndex(rectangle, int.MaxValue);
            Api.Client.UI.LayoutRootChildren.Add(rectangle);

            rectangle.Fill = Client.UI.GetTextureBrush(resultAtlasTexture);
        }
    }
}