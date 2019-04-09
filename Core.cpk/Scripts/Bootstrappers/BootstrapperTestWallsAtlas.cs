namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BootstrapperTestWallsAtlas : BaseBootstrapper
    {
        //public override async void ClientInitialize()
        //{
        //	await CreateTestTexturePreviewer();
        //}

        //private static async Task CreateTestTexturePreviewer()
        //{
        //    var protoWall = Api.GetProtoEntity<ObjectWallWood>();
        //    var atlasTexture = protoWall.TextureOverlayAtlas;
        //    var maskTexture = protoWall.TextureMaskAtlas;
        //    var request = new ProceduralTextureRequest();
        //
        //    var resultAtlasTexture =
        //        await WallTextureComposer.GenerateProceduralTextureNonAtlas(
        //            "Test",
        //            WallTextureComposer.WallPrimaryChunkTypes,
        //            atlasTexture,
        //            maskTexture,
        //            request);
        //
        //    var rectangle = new Rectangle
        //    {
        //        Width = WallTextureComposer.PrimaryTextureWidth,
        //        Height = WallTextureComposer.PrimaryTextureHeight,
        //        //Fill = ClientApi.UI.CreateImageBrushForRenderTarget(camera.RenderTarget),
        //        Stretch = Stretch.Uniform,
        //        UseLayoutRounding = true,
        //    };
        //
        //    Panel.SetZIndex(rectangle, int.MaxValue);
        //    Api.Client.UI.LayoutRootChildren.Add(rectangle);
        //
        //    rectangle.Fill = Client.UI.GetTextureBrush(resultAtlasTexture);
        //}
    }
}