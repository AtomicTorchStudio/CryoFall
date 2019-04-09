namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class WorldMapTexturesProvider
    {
        public const ushort WorldChunkMapTextureSize = WorldTileTextureSize * ScriptingConstants.WorldChunkSize;

        public const int WorldTileTextureSize = 4;

        private const int MaxSimultaneouslyLoadingChunks = 16;

        private static readonly ICoreClientService ClientCoreService = Api.Client.Core;

        private static readonly IUIClientService ClientUIService = Api.Client.UI;

        private static readonly IWorldClientService ClientWorldService = Api.Client.World;

        // TODO: fix memory leak - garbage collection for these image brushes will never happen!
        private static readonly Dictionary<uint, Task<TextureBrush>> TexturesCache
            = new Dictionary<uint, Task<TextureBrush>>();

        private static int activeTasksCount;

        public static bool IsBusy => activeTasksCount > MaxSimultaneouslyLoadingChunks
                                     || ClientCoreService.IsMainThreadIsOutOfTime;

        public static Task<TextureBrush> LoadMapChunkImageBrush(
            Vector2Ushort chunkStartPosition,
            uint checksum,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            if (TexturesCache.TryGetValue(checksum, out var result))
            {
                return result;
            }

            if (Api.Client.CurrentGame.ConnectionState
                != ConnectionState.Connected)
            {
                // not connected - don't load the map
                return null;
            }

            var tempTilesList = Api.Shared.GetTempList<Tile>();
            tempTilesList.AddRange(ClientWorldService.GetWorldChunk(chunkStartPosition));

            result = LoadAsync();
            TexturesCache.Add(checksum, result);
            return result;

            async Task<TextureBrush> LoadAsync()
            {
                activeTasksCount++;

                // create new image brush task
                var proceduralTexture = new ProceduralTexture(
                    "WorldMapChunk checksum=" + checksum,
                    // ReSharper disable once AccessToDisposedClosure
                    request => GenerateChunkProceduralTexture(tempTilesList, chunkStartPosition, request),
                    isTransparent: false,
                    isUseCache: false);

                var brush = ClientUIService.GetTextureBrush(proceduralTexture, Stretch.Fill);
                //brush = ClientUIService.GetTextureBrush(new TextureResource("TestWhiteRect", isTransparent: false), Stretch.None);

                try
                {
                    await brush.WaitLoaded();
                }
                finally
                {
                    activeTasksCount--;
                }

                return brush;
            }
        }

        private static async Task<ITextureResource> GenerateChunkProceduralTexture(
            ITempList<Tile> tiles,
            Vector2Ushort chunkStartPosition,
            ProceduralTextureRequest request)
        {
            var renderingService = Api.Client.Rendering;
            var renderingTag = request.TextureName;

            var textureSize = new Vector2Ushort(WorldChunkMapTextureSize, WorldChunkMapTextureSize);

            // create camera and render texture
            var renderTexture = renderingService.CreateRenderTexture(renderingTag,
                                                                     textureSize.X,
                                                                     textureSize.Y);
            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = renderingService.CreateCamera(cameraObject,
                                                       renderingTag,
                                                       drawOrder: -100);
            camera.RenderTarget = renderTexture;
            camera.ClearColor = Colors.Magenta; // to make potential issues visible clear with magenta color
            camera.SetOrthographicProjection(textureSize.X, textureSize.Y);

            // create tile renderers
            foreach (var tile in tiles)
            {
                var drawPosition = tile.Position.ToVector2D() - chunkStartPosition.ToVector2D();
                drawPosition = (
                                   drawPosition.X * WorldTileTextureSize,
                                   // Y is reversed
                                   (drawPosition.Y - ScriptingConstants.WorldChunkSize + 1) * WorldTileTextureSize);

                renderingService.CreateSpriteRenderer(
                    cameraObject,
                    tile.ProtoTile.GetWorldMapTexture(tile),
                    positionOffset: drawPosition,
                    // draw down
                    spritePivotPoint: (0, 1),
                    renderingTag: renderingTag);
            }

            tiles.Dispose();

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(isTransparent: false);
            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }
    }
}