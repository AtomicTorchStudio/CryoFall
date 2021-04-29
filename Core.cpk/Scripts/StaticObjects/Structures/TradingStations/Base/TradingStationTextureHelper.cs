namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class TradingStationTextureHelper
    {
        // throttle loading trading station textures at one station at a time
        private static readonly SemaphoreSlim TasksSemaphore = new(1, 1);

        public static ProceduralTexture CreateProceduralTexture(IStaticWorldObject worldObject)
        {
            return new(
                worldObject.ProtoGameObject.Id + " ID=" + worldObject.Id,
                isTransparent: true,
                isUseCache: true,
                generateTextureCallback:
                request => ClientGenerateProceduralTextureForTradingStationContent(
                    worldObject,
                    request));
        }

        private static async Task<ITextureResource> ClientGenerateProceduralTextureForTradingStationContent(
            IStaticWorldObject worldObject,
            ProceduralTextureRequest request)
        {
            var taskAcquired = TasksSemaphore.WaitAsync(Api.CancellationToken);

            try
            {
                await taskAcquired;

                await Api.Client.Core.YieldIfMainThreadIsOutOfTime();

                var rendering = Api.Client.Rendering;
                var renderingTag = request.TextureName;

                var qualityScaleCoef = rendering.CalculateCurrentQualityScaleCoefWithOffset(-1);
                var scale = 1.0 / qualityScaleCoef;

                var publicState = worldObject.GetPublicState<ObjectTradingStationPublicState>();
                var protoObjectTradingStation = ((IProtoObjectTradingStation)publicState.GameObject.ProtoGameObject);
                var controlWidth = (int)Math.Floor(90 * (protoObjectTradingStation.LotsCount / 2.0));
                var controlHeight = 268;

                // create and prepare UI renderer for the sign text to render
                var control = new ObjectTradingStationDisplayControl
                {
                    IsBuyMode = publicState.Mode == TradingStationMode.StationBuying,
                    TradingLots = publicState.Lots
                                             .Select(l => new TradingStationsMapMarksSystem.TradingStationLotInfo(l))
                                             .ToArray(),
                    Width = controlWidth,
                    Height = controlHeight,
                    LayoutTransform = new ScaleTransform(scale, scale)
                };

                var textureSize = new Vector2Ushort((ushort)(scale * controlWidth),
                                                    (ushort)(scale * controlHeight));

                // create camera and render texture
                var renderTexture = rendering.CreateRenderTexture(renderingTag,
                                                                  textureSize.X,
                                                                  textureSize.Y);
                var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
                var camera = rendering.CreateCamera(cameraObject,
                                                    renderingTag,
                                                    drawOrder: -200);
                camera.RenderTarget = renderTexture;
                camera.SetOrthographicProjection(textureSize.X, textureSize.Y);

                await Api.Client.Core.YieldIfMainThreadIsOutOfTime();

                rendering.CreateUIElementRenderer(
                    cameraObject,
                    control,
                    size: textureSize,
                    renderingTag: renderingTag);

                await Api.Client.Core.YieldIfMainThreadIsOutOfTime();

                await camera.DrawAsync();
                cameraObject.Destroy();

                await Api.Client.Core.YieldIfMainThreadIsOutOfTime();

                request.ThrowIfCancelled();

                var generatedTexture = await renderTexture.SaveToTexture(isTransparent: true,
                                                                         qualityScaleCoef: qualityScaleCoef);
                renderTexture.Dispose();
                request.ThrowIfCancelled();
                return generatedTexture;
            }
            finally
            {
                if (taskAcquired.IsCompleted)
                {
                    TasksSemaphore.Release();
                }
            }
        }
    }
}