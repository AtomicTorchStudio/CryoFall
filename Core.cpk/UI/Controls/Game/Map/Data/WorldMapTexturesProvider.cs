namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class WorldMapTexturesProvider
    {
        public const ushort WorldChunkMapTextureSize = WorldTileTextureSize * ScriptingConstants.WorldChunkSize;

        public const int WorldTileTextureSize = 4;

        // the limit is not actually required (don't affect the performance much)
        private const int MaxSimultaneouslyLoadingChunks = 128;

        private static readonly ICoreClientService ClientCoreService = Api.Client.Core;

        private static readonly IUIClientService ClientUIService = Api.Client.UI;

        private static readonly IWorldClientService ClientWorldService = Api.Client.World;

        // TODO: fix memory leak - garbage collection for these image brushes will never happen!
        private static readonly Dictionary<uint, TextureBrush> TexturesCache
            = new Dictionary<uint, TextureBrush>();

        private static readonly Dictionary<uint, Task<TextureBrush>> TexturesCacheActiveTasks
            = new Dictionary<uint, Task<TextureBrush>>();

        public static bool IsBusy => TexturesCacheActiveTasks.Count > MaxSimultaneouslyLoadingChunks
                                     || ClientCoreService.IsMainThreadIsOutOfTime;

        public static ValueTask<TextureBrush> LoadMapChunkImageBrush(
            Vector2Ushort chunkStartPosition,
            uint checksum,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new ValueTask<TextureBrush>(result: null);
            }

            if (TexturesCache.TryGetValue(checksum, out var result))
            {
                return new ValueTask<TextureBrush>(result);
            }

            if (TexturesCacheActiveTasks.TryGetValue(checksum, out var activeTask))
            {
                return new ValueTask<TextureBrush>(activeTask);
            }

            if (Api.Client.CurrentGame.ConnectionState
                != ConnectionState.Connected)
            {
                // not connected - don't load the map
                return new ValueTask<TextureBrush>(result: null);
            }

            var task = LoadAsync();
            if (!task.IsCompleted)
            {
                TexturesCacheActiveTasks.Add(checksum, task);
            }

            return new ValueTask<TextureBrush>(task);

            async Task<TextureBrush> LoadAsync()
            {
                TextureBrush brush;

                try
                {
                    // create new image brush task
                    var proceduralTexture = new ProceduralTexture(
                        "WorldMapChunk",
                        // ReSharper disable once AccessToDisposedClosure
                        GenerateChunkProceduralTexture,
                        isTransparent: false,
                        isUseCache: false,
                        useThrottling: false,
                        data: (checksum, chunkStartPosition));

                    brush = ClientUIService.GetTextureBrush(proceduralTexture, Stretch.Fill);
                    //brush = ClientUIService.GetTextureBrush(new TextureResource("TestWhiteRect", isTransparent: false), Stretch.None);

                    await brush.WaitLoaded();
                    TexturesCache.Add(checksum, brush);
                }
                finally
                {
                    TexturesCacheActiveTasks.Remove(checksum);
                }

                return brush;
            }
        }

        private static async Task<ITextureResource> GenerateChunkProceduralTexture(ProceduralTextureRequest request)
        {
            var (checksum, chunkStartPosition) = ((uint checksum, Vector2Ushort chunkStartPosition))request.Data;
            using var tempTilesList = Api.Shared.WrapInTempList(ClientWorldService.GetWorldChunk(chunkStartPosition));

            var renderingService = Api.Client.Rendering;
            var renderingTag = request.TextureName + "-" + checksum;

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
            camera.ClearColor = Colors.Magenta; // to make any potential issues obvious
            camera.SetOrthographicProjection(textureSize.X, textureSize.Y);

            // create tile renderers
            foreach (var tile in tempTilesList.EnumerateAndDispose())
            {
                var drawPosition = tile.Position.ToVector2D() - chunkStartPosition.ToVector2D();
                drawPosition = (drawPosition.X * WorldTileTextureSize,
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