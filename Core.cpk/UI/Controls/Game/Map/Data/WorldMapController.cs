namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using static WorldMapTexturesProvider;

    public class WorldMapController : IDisposable
    {
        private const ushort SectorSize = 100;

        private static readonly IWorldClientService WorldService = Api.Client.World;

        public Action<Vector2D> MapClickCallback;

        private readonly CancellationToken cancellationToken;

        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly Canvas canvasMap;

        private readonly UIElementCollection canvasMapChildren;

        private readonly Dictionary<Vector2Ushort, SectorControl> canvasMapSectorControls;

        private readonly List<UIElement> extraControls = new List<UIElement>();

        private readonly HashSet<Vector2Ushort> newChunksHashSet
            = new HashSet<Vector2Ushort>();

        private readonly PanningPanel panningPanel;

        private readonly List<Vector2Ushort> queueChunks
            = new List<Vector2Ushort>(capacity: 10000);

        private readonly ViewModelControlWorldMap viewModelControlWorldMap;

        private ClientInputContext clintInputContext;

        private ClientComponentWorldMapCurrentCharacterUpdater componentCurrentCharacterUpdater;

        private bool isActive;

        private bool isAutocenterOnPlayer = true;

        private bool isDisposed;

        private bool isVisibleMapBoundsDirty = true;

        private Vector2Ushort lastCurrentMapPosition;

        private double lastExtraControlsScale = 1;

        private Vector2D? lastPlayerCanvasPosition;

        private Vector2Ushort lastPointedMapPosition;

        private IClientSceneObject sceneObject;

        private double? timeForNextUpdate = 0;

        private BoundsUshort worldBounds;

        public WorldMapController(
            PanningPanel panningPanel,
            ViewModelControlWorldMap viewModelControlWorldMap,
            bool isEditorMap)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken = CancellationTokenSource
                                     .CreateLinkedTokenSource(this.cancellationTokenSource.Token, Api.CancellationToken)
                                     .Token;

            this.panningPanel = panningPanel;
            this.viewModelControlWorldMap = viewModelControlWorldMap;
            this.IsEditorMap = isEditorMap;
            this.canvasMap = new Canvas();
            this.canvasMapChildren = this.canvasMap.Children;
            this.canvasMapSectorControls = new Dictionary<Vector2Ushort, SectorControl>();

            this.panningPanel.Items.Clear();
            this.panningPanel.Items.Add(this.canvasMap);

            // create current character display
            this.CreateCurrentCharacterControl();

            // create current camera view bounds display
            var controlCurrentCameraView = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(0x33,   0xFF, 0xFF, 0xFF)),
                Stroke = new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF)),
                StrokeThickness = 1
            };

            Panel.SetZIndex(controlCurrentCameraView, 10);
            if (this.IsEditorMap)
            {
                this.canvasMapChildren.Add(controlCurrentCameraView);
            }

            this.sceneObject.AddComponent<ClientComponentWorldMapCurrentCameraViewUpdated>()
                .Setup(this, controlCurrentCameraView, WorldTileTextureSize);

            panningPanel.CallbackGetSliderZoomCanvasPosition =
                () => this.isAutocenterOnPlayer
                          ? this.componentCurrentCharacterUpdater.CanvasPosition
                          : (Vector2D?)null;

            // setup events
            WorldService.WorldChunkAddedOrUpdated += this.WorldChunkAddedOrUpdatedHandler;
            WorldService.WorldBoundsChanged += this.WorldBoundsChangedHandler;
            panningPanel.MouseHold += this.PanningPanelMouseHoldHandler;
            panningPanel.MouseLeftButtonClick += this.PanningPanelMouseLeftButtonClickHandler;
            panningPanel.ZoomChanged += this.PanningPanelZoomChangedHandler;

            ClientUpdateHelper.UpdateCallback += this.Update;

            // init map
            this.InitMap();
        }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                if (this.isActive)
                {
                    this.clintInputContext?.Stop();
                    this.clintInputContext = null;
                }

                this.isActive = value;

                if (this.isActive)
                {
                    this.clintInputContext =
                        ClientInputContext.Start(nameof(WorldMapController) + (this.IsEditorMap ? "_Editor" : "_Game"))
                                          .HandleAll(() =>
                                                     {
                                                         if (Api.Client.Input.IsKeyDown(InputKey.Space))
                                                         {
                                                             this.CenterMapOnPlayerCharacter();
                                                         }
                                                     });
                }
            }
        }

        public bool IsEditorMap { get; }

        public Vector2Ushort PointedMapPositionWithOffset
            => this.PointedMapPositionWithoutOffset - this.worldBounds.Offset;

        public Vector2Ushort PointedMapPositionWithoutOffset
            => this.ScreenToWorldPosition();

        public static IEnumerable<Vector2Ushort> OrderChunksByProximityToPlayer(
            IEnumerable<Vector2Ushort> worldChunksAvailable)
        {
            var playerCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            if (playerCharacter == null)
            {
                return worldChunksAvailable;
            }

            var playerPosition = playerCharacter.TilePosition;
            return worldChunksAvailable.OrderBy(c => c.TileDistanceTo(playerPosition));
        }

        public void AddControl(UIElement control, bool scaleWithZoom = true)
        {
            this.extraControls.Add(control);
            if (scaleWithZoom)
            {
                var scale = this.lastExtraControlsScale;
                control.RenderTransform = new ScaleTransform(scale, scale);
            }

            this.canvasMapChildren.Add(control);
        }

        public void CenterMapOnPlayerCharacter()
        {
            this.isAutocenterOnPlayer = true;
            var canvasPosition = this.componentCurrentCharacterUpdater.CanvasPosition;
            this.lastPlayerCanvasPosition = canvasPosition;
            //Api.Logger.Write("Centering on player at canvas position: " + canvasPosition);

            if (this.panningPanel.CurrentTargetZoom < 0.5)
            {
                this.panningPanel.SetZoom(0.5);
            }

            this.panningPanel.CenterOnPoint(canvasPosition);
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                throw new Exception("Already disposed");
            }

            this.isDisposed = true;
            this.IsActive = false;

            this.sceneObject.Destroy();
            this.queueChunks.Clear();
            this.newChunksHashSet.Clear();
            WorldService.WorldChunkAddedOrUpdated -= this.WorldChunkAddedOrUpdatedHandler;
            WorldService.WorldBoundsChanged -= this.WorldBoundsChangedHandler;
            this.panningPanel.MouseHold -= this.PanningPanelMouseHoldHandler;
            this.panningPanel.MouseLeftButtonClick -= this.PanningPanelMouseLeftButtonClickHandler;
            ClientUpdateHelper.UpdateCallback -= this.Update;

            this.cancellationTokenSource.Cancel();
        }

        public void RemoveControl(UIElement control)
        {
            if (this.extraControls.Remove(control))
            {
                this.canvasMapChildren.Remove(control);
            }
        }

        public Vector2Ushort ScreenToWorldPosition()
        {
            var canvasPosition = Mouse.GetPosition(this.canvasMap);
            var x = canvasPosition.X / WorldTileTextureSize;
            var y = canvasPosition.Y / WorldTileTextureSize;

            x = MathHelper.Clamp(x, 0, this.worldBounds.Size.X);
            y = MathHelper.Clamp(y, 0, this.worldBounds.Size.Y);

            y = this.worldBounds.Size.Y - y;

            x += this.worldBounds.Offset.X;
            y += this.worldBounds.Offset.Y;

            return new Vector2Ushort((ushort)Math.Round(x),
                                     (ushort)Math.Round(y));
        }

        public Vector2D WorldToCanvasPosition(Vector2D worldPosition)
        {
            var x = worldPosition.X - this.worldBounds.Offset.X;
            var y = this.worldBounds.Offset.Y - worldPosition.Y;
            y += this.worldBounds.Size.Y;

            return (x * WorldTileTextureSize, y * WorldTileTextureSize);
        }

        private static async void Load(
            Rectangle tileRectangle,
            Vector2Ushort chunkStartPosition,
            uint checksum,
            CancellationToken cancellationToken)
        {
            //Api.Logger.Write("Loading tile brush: " + chunkStartPosition + " / "  +checksum);
            var imageBrush = await LoadMapChunkImageBrush(chunkStartPosition, checksum, cancellationToken);
            if (imageBrush == null)
            {
                // cancelled
                return;
            }

            // verify if this is an actual chunk
            var currentChecksum = WorldService.GetWorldChunkChecksum(chunkStartPosition);
            if (currentChecksum == checksum)
            {
                //Api.Logger.Write("Assigning tile brush: " + chunkStartPosition + " / "  +checksum);
                tileRectangle.Fill = imageBrush;
            }
        }

        private void AddOrRefreshChunk(Vector2Ushort chunkStartPosition)
        {
            var sectorPosition = this.CalculateSectorPosition(chunkStartPosition);
            if (!this.canvasMapSectorControls.TryGetValue(sectorPosition, out var sector))
            {
                var sectorCanvas = new Canvas();
                //var sectorVisualSize = SectorSize * WorldChunkMapTextureSize;
                //sectorCanvas.Width = sectorVisualSize;
                //sectorCanvas.Height = sectorVisualSize;
                var sectorVisualPosition = this.WorldToCanvasPosition(sectorPosition.ToVector2D());
                Canvas.SetLeft(sectorCanvas, sectorVisualPosition.X);
                Canvas.SetTop(sectorCanvas, sectorVisualPosition.Y);

                sector = new SectorControl(sectorCanvas, sectorPosition);
                this.canvasMapChildren.Add(sectorCanvas);
                this.canvasMapSectorControls.Add(sectorPosition, sector);
            }

            var checksum = WorldService.GetWorldChunkChecksum(chunkStartPosition);
            if (sector.TryGetTileRectangle(chunkStartPosition, out var tileRectangle))
            {
                // already added - need to refresh the texture
                Load(tileRectangle, chunkStartPosition, checksum, this.cancellationToken);
                return;
            }

            // need to create a new rectangle and add it to Canvas
            // to fix NoesisGUI rendering issue (1 px holes between rectangles)
            const double squareSize = WorldChunkMapTextureSize + 1;
            tileRectangle = new Rectangle
            {
                Width = squareSize,
                Height = squareSize,
            };
            //RenderOptions.SetBitmapScalingMode(tileRectangle, BitmapScalingMode.NearestNeighbor);

            var chunkCanvasPosition = this.WorldToCanvasPosition(chunkStartPosition.ToVector2D())
                                      - this.WorldToCanvasPosition(sectorPosition.ToVector2D());

            Canvas.SetLeft(tileRectangle, chunkCanvasPosition.X);
            Canvas.SetTop(tileRectangle, chunkCanvasPosition.Y - squareSize);

            sector.AddTileRectangle(chunkStartPosition, tileRectangle);
            Load(tileRectangle, chunkStartPosition, checksum, this.cancellationToken);
        }

        private double CalculateExtraControlsScale()
        {
            return 1 / this.panningPanel.CurrentAnimatedZoom;
        }

        private Vector2Ushort CalculateSectorPosition(in Vector2Ushort position)
        {
            return new Vector2Ushort((ushort)((position.X / SectorSize) * SectorSize),
                                     (ushort)((position.Y / SectorSize) * SectorSize));
        }

        private Vector2D CanvasToWorldPosition(Vector2D canvasPosition)
        {
            var x = canvasPosition.X / WorldTileTextureSize;
            var y = canvasPosition.Y / WorldTileTextureSize;
            y -= this.worldBounds.Size.Y;

            x += this.worldBounds.Offset.X;
            y = this.worldBounds.Offset.Y - y;

            return (x, y);
        }

        private void CreateCurrentCharacterControl()
        {
            var controlCurrentCharacter = new WorldMapMarkCurrentCharacter();
            Panel.SetZIndex(controlCurrentCharacter, 10);
            if (!this.IsEditorMap)
            {
                // show current character only on the non-Editor map
                this.AddControl(controlCurrentCharacter);
            }

            this.sceneObject = Api.Client.Scene.CreateSceneObject("World map updater");
            this.componentCurrentCharacterUpdater =
                this.sceneObject.AddComponent<ClientComponentWorldMapCurrentCharacterUpdater>();
            this.componentCurrentCharacterUpdater.Setup(this, controlCurrentCharacter);
        }

        private void InitMap()
        {
            this.worldBounds = WorldService.WorldBounds;
            this.panningPanel.PanningWidth = this.worldBounds.Size.X * WorldTileTextureSize;
            this.panningPanel.PanningHeight = this.worldBounds.Size.Y * WorldTileTextureSize;

            foreach (var sectorControl in this.canvasMapSectorControls.Values)
            {
                this.canvasMapChildren.Remove(sectorControl.Canvas);
            }

            this.canvasMapSectorControls.Clear();

            this.queueChunks.Clear();
            this.newChunksHashSet.Clear();
            this.queueChunks.AddRange(WorldService.AvailableWorldChunks);

            this.isVisibleMapBoundsDirty = true;
            this.UpdateMapBoundsIfDirty();

            //Api.Logger.WriteDev($"Map init: Editor={this.IsEditor}, total world chunks available count: {World.WorldChunksAvailable.Count}");
        }

        private void PanningPanelMouseHoldHandler()
        {
            this.isAutocenterOnPlayer = false;
        }

        private void PanningPanelMouseLeftButtonClickHandler(PanningPanel s, MouseEventArgs e)
        {
            var canvasPosition = Mouse.GetPosition(this.canvasMap).ToVector2D();
            var worldPosition = this.CanvasToWorldPosition(canvasPosition);
            this.MapClickCallback?.Invoke(worldPosition);
        }

        private void PanningPanelZoomChangedHandler((double newZoom, bool isByPlayersInput) args)
        {
            if (args.isByPlayersInput)
            {
                this.isAutocenterOnPlayer = false;
            }
        }

        // Every frame the game will perform an update and determine which chunks it need to add on the map.
        // This is essential task scheduling operation - otherwise world map textures provider will be overflooded with active tasks.
        private void Update()
        {
            if (!this.IsActive)
            {
                // update world map only when it's active (visible)
                this.lastCurrentMapPosition = Vector2Ushort.Max;
                return;
            }

            if (Api.Client.CurrentGame.ConnectionState
                != ConnectionState.Connected)
            {
                // not connected - don't update the map
                return;
            }

            this.UpdateMapBoundsIfDirty();
            this.UpdateExtraControlsZoom();

            this.UpdateMapExplorationProgress();

            var currentWorldPosition = this.componentCurrentCharacterUpdater.WorldPosition.ToVector2Ushort();
            var currentMapPosition = currentWorldPosition - this.worldBounds.Offset;
            if (currentMapPosition != this.lastCurrentMapPosition)
            {
                this.lastCurrentMapPosition = currentMapPosition;
                this.viewModelControlWorldMap.CurrentPositionText = currentMapPosition.ToString();
                if (this.newChunksHashSet.Count == 0)
                {
                    // reorder queue to load chunks which are closer to the character first
                    this.queueChunks.SortBy(c => c.TileDistanceTo(currentWorldPosition));
                }
                else
                {
                    // the queue will be reordered later
                }
            }

            var pointedMapPosition = this.PointedMapPositionWithOffset;
            if (pointedMapPosition != this.lastPointedMapPosition)
            {
                this.lastPointedMapPosition = pointedMapPosition;
                this.viewModelControlWorldMap.PointedPositionText = pointedMapPosition.ToString();
            }

            // helper local func to center map on the player position
            void AutoCenterMapOnPlayerCharacter()
            {
                // invoke this method only when the window is active
                if (!this.isAutocenterOnPlayer)
                {
                    return;
                }

                var canvasPosition = this.componentCurrentCharacterUpdater.CanvasPosition;
                if (this.lastPlayerCanvasPosition == canvasPosition)
                {
                    // player position didn't changed
                    return;
                }

                this.CenterMapOnPlayerCharacter();
            }

            AutoCenterMapOnPlayerCharacter();

            if (IsBusy)
            {
                return;
            }

            if (this.newChunksHashSet.Count > 0)
            {
                // rebuild queue
                if (this.queueChunks.Count > 0)
                {
                    this.newChunksHashSet.AddRange(this.queueChunks);
                    this.queueChunks.Clear();
                }

                this.queueChunks.AddRange(this.newChunksHashSet);
                this.newChunksHashSet.Clear();
                this.queueChunks.SortBy(c => c.TileDistanceTo(currentWorldPosition));
            }

            if (this.queueChunks.Count == 0
                || IsBusy)
            {
                return;
            }

            if (this.timeForNextUpdate.HasValue)
            {
                if (Api.Client.Core.ClientRealTime < this.timeForNextUpdate)
                {
                    // wait
                    return;
                }

                // time for the update
                this.timeForNextUpdate = null;
            }

            var index = 0;
            do
            {
                var chunkStartPosition = this.queueChunks[index];
                this.AddOrRefreshChunk(chunkStartPosition);
                index++;

                if (IsBusy)
                {
                    break;
                }
            }
            while (index < this.queueChunks.Count);

            // remove processed chunks
            this.queueChunks.RemoveRange(0, index);
        }

        private void UpdateExtraControlsZoom()
        {
            var scale = this.CalculateExtraControlsScale();
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (this.lastExtraControlsScale == scale)
            {
                return;
            }

            this.lastExtraControlsScale = scale;
            foreach (var extraControl in this.extraControls)
            {
                var transform = extraControl.RenderTransform;
                if (transform is ScaleTransform scaleTransform)
                {
                    scaleTransform.ScaleX = scaleTransform.ScaleY = scale;
                }
            }
        }

        private void UpdateMapBoundsIfDirty()
        {
            if (!this.isVisibleMapBoundsDirty)
            {
                return;
            }

            this.isVisibleMapBoundsDirty = false;

            BoundsUshort boundsVisible;

            if (true) //this.IsEditorMap)
            {
                boundsVisible = this.worldBounds;
            }
            else // TODO: temporary disabled
                // we need a better algorithm to calculate the max "observable" world bounds with proper padding
                // otherwise the map is moving when new world chunks are explored and bounds extended
            {
                var allChunks = WorldService.AvailableWorldChunks.ToList();
                if (!allChunks.Any())
                {
                    return;
                }

                ushort maxX = 0,
                       maxY = 0,
                       minX = ushort.MaxValue,
                       minY = ushort.MaxValue;

                foreach (var chunk in allChunks)
                {
                    if (chunk.X > maxX)
                    {
                        maxX = chunk.X;
                    }

                    if (chunk.X < minX)
                    {
                        minX = chunk.X;
                    }

                    if (chunk.Y > maxY)
                    {
                        maxY = chunk.Y;
                    }

                    if (chunk.Y < minY)
                    {
                        minY = chunk.Y;
                    }
                }

                maxX += ScriptingConstants.WorldChunkSize;
                maxY += ScriptingConstants.WorldChunkSize;

                boundsVisible = new BoundsUshort(minX, minY, maxX, maxY);
            }

            // apply padding
            {
                var padding = (this.IsEditorMap ? 1 : 0) * ScriptingConstants.WorldChunkSize;
                var minX = (ushort)Math.Max(0, boundsVisible.MinX - padding);
                var minY = (ushort)Math.Max(0, boundsVisible.MinY - padding);
                var maxX = (ushort)Math.Min(ushort.MaxValue, boundsVisible.MaxX + padding);
                var maxY = (ushort)Math.Min(ushort.MaxValue, boundsVisible.MaxY + padding);
                boundsVisible = new BoundsUshort(minX, minY, maxX, maxY);
            }

            // don't allow scale closer than that
            this.panningPanel.MaxZoom = 1;

            // calculate how much it's possible to zoom out
            this.panningPanel.IsAutoCalculatingMinZoom = true;

            this.panningPanel.PanningBounds = new BoundsDouble(
                minX: this.WorldToCanvasPosition((boundsVisible.MinX, 0)).X,
                minY: this.WorldToCanvasPosition((0, boundsVisible.MaxY)).Y,
                maxX: this.WorldToCanvasPosition((boundsVisible.MaxX, 0)).X,
                maxY: this.WorldToCanvasPosition((0, boundsVisible.MinY)).Y);

            //this.panningPanel.SetZoom(this.panningPanel.CurrentTargetZoom);
        }

        private void UpdateMapExplorationProgress()
        {
            if (Api.Client.Characters.IsCurrentPlayerCharacterSpectator)
            {
                this.viewModelControlWorldMap.MapExploredPercent = 100;
                return;
            }

            var totalWorldChunksCount = WorldService.DiscoverableWorldChunksTotalCount;
            var percent = WorldService.DiscoveredWorldChunksCount / (double)totalWorldChunksCount;
            percent = Math.Min(percent, 1.0);
            this.viewModelControlWorldMap.MapExploredPercent = (byte)Math.Ceiling(100 * percent);
        }

        private void WorldBoundsChangedHandler()
        {
            this.InitMap();
        }

        private void WorldChunkAddedOrUpdatedHandler(Vector2Ushort chunkStartPosition)
        {
            this.newChunksHashSet.Add(chunkStartPosition);

            if (!this.timeForNextUpdate.HasValue)
            {
                // update map tiles after 0.5 second delay
                // this is very helpful when we edit map in the Editor as there could be multiple changes per second
                //this.timeForNextUpdate = Api.Client.Core.ClientRealTime + 0.5;
            }

            this.isVisibleMapBoundsDirty = true;
        }

        private class SectorControl
        {
            public readonly Canvas Canvas;

            public readonly Vector2Ushort SectorPosition;

            private readonly UIElementCollection canvasChildren;

            private readonly Dictionary<Vector2Ushort, Rectangle> chunkVisualizers
                = new Dictionary<Vector2Ushort, Rectangle>();

            public SectorControl(Canvas canvas, Vector2Ushort sectorPosition)
            {
                this.SectorPosition = sectorPosition;
                this.Canvas = canvas;
                this.canvasChildren = canvas.Children;
            }

            public void AddTileRectangle(in Vector2Ushort chunkStartPosition, Rectangle rectangle)
            {
                this.chunkVisualizers.Add(chunkStartPosition, rectangle);
                this.canvasChildren.Add(rectangle);
            }

            public bool TryGetTileRectangle(in Vector2Ushort chunkStartPosition, out Rectangle rectangle)
            {
                return this.chunkVisualizers.TryGetValue(chunkStartPosition, out rectangle);
            }
        }
    }
}