namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class WorldMapSectorProvider
    {
        public const ushort SectorWorldSize = 20 * ScriptingConstants.WorldChunkSize;

        private const int MaxChunksToRenderPerIteration = ((SectorWorldSize * SectorWorldSize)
                                                           / (ScriptingConstants.WorldChunkSize
                                                              * ScriptingConstants.WorldChunkSize))
                                                          / (2 * 4);

        private const int MaxRenderersPerIteration = MaxChunksToRenderPerIteration
                                                     * ScriptingConstants.WorldChunkSize
                                                     * ScriptingConstants.WorldChunkSize;

        public static readonly int WorldTileTextureSize
            = Api.Client.Rendering.SpriteQualitySizeMultiplierReverse > 2
                  ? 2 // use smaller map tiles (smaller map sector render texture) when sprite quality is low
                  : 4;

        public static readonly double PanningPanelZoomScale = (4.0 / WorldTileTextureSize);

        public static readonly ushort SectorPixelSize = (ushort)(SectorWorldSize * WorldTileTextureSize);

        private static readonly IWorldClientService World = Api.Client.World;

        private readonly ICamera2D camera;

        private readonly HashSet<SectorRenderer> queuedSectorsToRender = new();

        private readonly RenderersCache renderersCache;

        private readonly string renderingTag = $"World map sector renderer {RandomHelper.Next()}";

        private readonly Dictionary<Vector2Ushort, SectorRenderer> sectorRenderers
            = new();

        private readonly List<SectorRenderer> tempListUpdate = new();

        private SectorRenderer lastSectorRendererScheduledForDraw;

        private int usageCounter;

        public WorldMapSectorProvider()
        {
            Api.OnShutdown += this.ApiShutdownHandler;
            ClientUpdateHelper.UpdateCallback += this.Update;
            Api.Client.World.WorldBoundsChanged += this.WorldBoundsChangedHandler;

            var sceneObject = Api.Client.Scene.CreateSceneObject(this.renderingTag);
            this.camera = Api.Client.Rendering.CreateCamera(sceneObject,
                                                            this.renderingTag,
                                                            drawOrder: -100);
            this.camera.SetOrthographicProjection(SectorPixelSize, SectorPixelSize);
            this.camera.DrawMode = CameraDrawMode.Manual;

            this.renderersCache = new RenderersCache(this.renderingTag, sceneObject);
        }

        public int UsageCounter
        {
            get => this.usageCounter;
            set
            {
                if (this.usageCounter == value)
                {
                    return;
                }

                this.usageCounter = value;
                //Api.Logger.Dev("Usage counter changed: " + value);
            }
        }

        private bool IsBusy => this.lastSectorRendererScheduledForDraw?.IsBusy ?? false;

        public void AddOrRefreshChunk(SectorRenderer sectorRenderer, Vector2Ushort chunkStartPosition)
        {
            this.queuedSectorsToRender.Add(sectorRenderer);
            sectorRenderer.AddOrRefreshChunk(chunkStartPosition);
        }

        public SectorRenderer GetSectorRenderer(Vector2Ushort sectorPosition)
        {
            if (this.sectorRenderers.TryGetValue(sectorPosition, out var sectorRenderer))
            {
                return sectorRenderer;
            }

            sectorRenderer = new SectorRenderer(sectorPosition,
                                                this.renderersCache,
                                                this.camera);
            this.sectorRenderers[sectorPosition] = sectorRenderer;
            return sectorRenderer;
        }

        private void ApiShutdownHandler()
        {
            foreach (var renderer in this.sectorRenderers.Values)
            {
                renderer.CancelRendering();
            }
        }

        private void Update()
        {
            if (this.IsBusy)
            {
                return;
            }

            if (this.UsageCounter == 0)
            {
                // pause all activity as there is no usage of this provider
                this.renderersCache.OnNothingToRender();
                return;
            }

            if (this.queuedSectorsToRender.Count == 0)
            {
                this.renderersCache.OnNothingToRender();
                return;
            }

            var playerCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            if (playerCharacter is null)
            {
                this.renderersCache.OnNothingToRender();
                return;
            }

            this.tempListUpdate.AddRange(this.queuedSectorsToRender);
            var centerPos =
                (playerCharacter.TilePosition - new Vector2Int(SectorWorldSize / 2, SectorWorldSize / 2))
                .ToVector2Ushort();

            this.tempListUpdate.SortBy(c => c.SectorPosition.TileDistanceTo(centerPos));
            this.lastSectorRendererScheduledForDraw = this.tempListUpdate[0];
            this.lastSectorRendererScheduledForDraw.AllowRendering();
            this.queuedSectorsToRender.Remove(this.lastSectorRendererScheduledForDraw);
            this.tempListUpdate.Clear();

            /*Api.Logger.Dev("Will render sector: "
                           + this.lastSectorRendererScheduledForDraw.SectorPosition
                           + " sectors to render remains: "
                           + this.queuedSectorsToRender.Count);*/
        }

        private void WorldBoundsChangedHandler()
        {
            foreach (var renderer in this.sectorRenderers.Values)
            {
                renderer.Dispose();
            }

            this.sectorRenderers.Clear();
            this.queuedSectorsToRender.Clear();
            this.lastSectorRendererScheduledForDraw = null;
            // it will reset the cache
            this.renderersCache.OnNothingToRender();
        }

        public class SectorRenderer
        {
            public readonly IRenderTarget2D RenderTexture;

            public readonly Vector2Ushort SectorPosition;

            private readonly ICamera2D camera;

            private readonly HashSet<Vector2Ushort> chunksToRender
                = new();

            private readonly RenderersCache renderersCache;

            private bool isDisposed;

            private bool isDrawingScheduled;

            private bool requiresCameraClear = true;

            private TaskCompletionSource<bool> taskCompletionSourceCanDraw
                = new();

            internal SectorRenderer(
                Vector2Ushort sectorPosition,
                RenderersCache renderersCache,
                ICamera2D camera)
            {
                this.SectorPosition = sectorPosition;
                this.renderersCache = renderersCache;
                this.camera = camera;

                this.RenderTexture = Api.Client.Rendering.CreateRenderTexture(
                    "World map sector: " + sectorPosition,
                    SectorPixelSize,
                    SectorPixelSize,
                    SurfaceFormat.Color);
            }

            /// <summary>
            /// This is correct code. The sector renderer is busy until
            /// it doesn't reset the taskCompletionSourceCanDraw with a new one.
            /// </summary>
            public bool IsBusy => this.taskCompletionSourceCanDraw.Task.IsCompleted;

            public void AllowRendering()
            {
                this.taskCompletionSourceCanDraw.SetResult(true);
                this.ScheduleDrawing();
            }

            public void CancelRendering()
            {
                if (this.taskCompletionSourceCanDraw.Task.Status == TaskStatus.Created)
                {
                    this.taskCompletionSourceCanDraw.SetResult(false);
                }

                this.taskCompletionSourceCanDraw = new TaskCompletionSource<bool>();
            }

            public void Dispose()
            {
                if (this.isDisposed)
                {
                    return;
                }

                this.isDisposed = true;
                this.CancelRendering();
                this.RenderTexture.Dispose();
            }

            protected internal void AddOrRefreshChunk(Vector2Ushort chunkStartPosition)
            {
                this.chunksToRender.Add(chunkStartPosition);
            }

            private static Vector2Ushort GetPlayerPosition()
            {
                var playerPosition = Api.Client.Characters.CurrentPlayerCharacter.Position.ToVector2Int()
                                     - (ScriptingConstants.WorldChunkSize / 2, ScriptingConstants.WorldChunkSize / 2);
                return ((ushort)Math.Max(0, playerPosition.X),
                        (ushort)Math.Max(0, playerPosition.Y));
            }

            private void CameraDrawAfter(IGraphicsDevice obj)
            {
                if (this.isDisposed)
                {
                    return;
                }

                this.renderersCache.OnDrawFinish();
                this.requiresCameraClear = false;
            }

            private void CameraDrawBefore(IGraphicsDevice obj)
            {
                //Api.Logger.Dev("Sector camera rendering preparation: " + this.renderingTag);
                var sectorPosition = this.SectorPosition.ToVector2D();
                var playerPosition = GetPlayerPosition();
                using var tempListChunksToRender
                    = Api.Shared.WrapInTempList((ICollection<Vector2Ushort>)this.chunksToRender);
                tempListChunksToRender.SortBy(p => p.TileSqrDistanceTo(playerPosition));
                {
                    var countToRemove = tempListChunksToRender.Count - MaxChunksToRenderPerIteration;
                    if (countToRemove > 0)
                    {
                        tempListChunksToRender.AsList().RemoveRange(MaxChunksToRenderPerIteration,
                                                                    count: countToRemove);
                    }
                }

                foreach (var chunkStartPosition in tempListChunksToRender.AsList())
                {
                    this.chunksToRender.Remove(chunkStartPosition);

                    if (!World.IsWorldChunkAvailable(chunkStartPosition))
                    {
                        continue;
                    }

                    try
                    {
                        var chunkOffset = chunkStartPosition.ToVector2Int() - sectorPosition.ToVector2Int();

                        // create tile renderers
                        foreach (var tile in World.GetWorldChunk(chunkStartPosition))
                        {
                            var drawPosition = (tile.Position - chunkStartPosition).ToVector2D();

                            drawPosition = (chunkOffset.X + drawPosition.X,
                                            // Y is reversed
                                            chunkOffset.Y + drawPosition.Y - SectorWorldSize + 1);

                            drawPosition *= WorldTileTextureSize;

                            var renderer = this.renderersCache.GetNext();
                            renderer.PositionOffset = drawPosition;
                            renderer.TextureResource = tile.ProtoTile.GetWorldMapTexture(tile);
                            renderer.IsEnabled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Api.Logger.Warning($"Cannot process world map chunk: {chunkStartPosition}: {ex.Message}");
                    }
                }

                this.renderersCache.OnDrawStart();
            }

            private async void ScheduleDrawing()
            {
                if (this.isDrawingScheduled)
                {
                    throw new Exception("Sector drawing already scheduled");
                }

                this.isDrawingScheduled = true;
                await ProtoTileMapTexturesPreloadHelper.Prepare();

                if (this.isDisposed)
                {
                    return;
                }

                while (this.chunksToRender.Count > 0)
                {
                    if (!await this.taskCompletionSourceCanDraw.Task)
                    {
                        break;
                    }

                    if (this.isDisposed)
                    {
                        return;
                    }

                    this.camera.RenderTarget = this.RenderTexture;
                    this.camera.ClearColor = this.requiresCameraClear
                                                 ? Colors.Black
                                                 : (Color?)null;
                    this.camera.BeforeDraw += this.CameraDrawBefore;
                    this.camera.AfterDraw += this.CameraDrawAfter;

                    try
                    {
                        await this.camera.DrawAsync();
                    }
                    finally
                    {
                        this.camera.BeforeDraw -= this.CameraDrawBefore;
                        this.camera.AfterDraw -= this.CameraDrawAfter;
                    }
                }

                if (this.isDisposed)
                {
                    return;
                }

                this.isDrawingScheduled = false;
                this.taskCompletionSourceCanDraw = new TaskCompletionSource<bool>();
            }
        }

        internal class RenderersCache
        {
            private readonly IComponentSpriteRenderer[] renderers
                = new IComponentSpriteRenderer[MaxRenderersPerIteration];

            private int lastUsedIndex = -1;

            private int previousLastUsedIndex = -1;

            public RenderersCache(string renderingTag, IClientSceneObject sceneObject)
            {
                var customRendererSize = WorldTileTextureSize == 4
                                             ? null
                                             : (double?)WorldTileTextureSize / 2f;
                for (var index = 0; index < MaxRenderersPerIteration; index++)
                {
                    var renderer = Api.Client.Rendering
                                      .CreateSpriteRenderer(
                                          sceneObject,
                                          textureResource: null,
                                          // draw down
                                          spritePivotPoint: (0, 1),
                                          renderingTag: renderingTag);
                    renderer.IsEnabled = false;
                    if (customRendererSize.HasValue)
                    {
                        renderer.Size = customRendererSize.Value;
                    }

                    this.renderers[index] = renderer;
                }
            }

            public IComponentSpriteRenderer GetNext()
            {
                return this.renderers[++this.lastUsedIndex];
            }

            public void OnDrawFinish()
            {
                this.previousLastUsedIndex = this.lastUsedIndex;
                this.lastUsedIndex = -1;
            }

            public void OnDrawStart()
            {
                // disable not used renderers since previous rendering
                this.DisableUnusedRenderers();
            }

            public void OnNothingToRender()
            {
                if (this.previousLastUsedIndex < 0)
                {
                    return;
                }

                this.lastUsedIndex = -1;
                this.DisableUnusedRenderers();
                this.previousLastUsedIndex = -1;
            }

            private void DisableUnusedRenderers()
            {
                for (var index = this.lastUsedIndex + 1;
                     index <= this.previousLastUsedIndex;
                     index++)
                {
                    this.renderers[index].IsEnabled = false;
                }
            }
        }
    }
}