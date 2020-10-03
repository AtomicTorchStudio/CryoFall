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
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class WorldMapController : IDisposable
    {
        private const ushort SectorSize = 100;

        protected static readonly IWorldClientService World = Api.Client.World;

        public Action<Vector2D> MapClickCallback;

        protected readonly Dictionary<Vector2Ushort, SectorControl> canvasMapSectorControls;

        protected readonly HashSet<Vector2Ushort> newChunksHashSet
            = new HashSet<Vector2Ushort>();

        protected readonly List<Vector2Ushort> queueChunks
            = new List<Vector2Ushort>(capacity: 10000);

        private readonly CancellationToken cancellationToken;

        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly Canvas canvasMap;

        private readonly UIElementCollection canvasMapChildren;

        private readonly List<UIElement> extraControls = new List<UIElement>();

        private readonly bool isListeningToInput;

        private readonly bool isPlayerMarkDisplayed;

        private readonly int paddingChunks;

        private readonly PanningPanel panningPanel;

        private readonly ViewModelControlWorldMap viewModelControlWorldMap;

        private ClientInputContext clintInputContext;

        private ClientComponentWorldMapCurrentCharacterUpdater componentCurrentCharacterUpdater;

        private ControlTemplate customControlTemplatePlayerMark;

        private bool isActive;

        private bool isAutocenterOnPlayer = true;

        private bool isCurrentCameraViewDisplayed;

        private bool isDisposed;

        private bool isVisibleMapBoundsDirty = true;

        private Vector2Ushort lastCurrentMapPosition;

        private double lastExtraControlsScale = 1;

        private Vector2D? lastPlayerCanvasPosition;

        private Vector2Ushort lastPointedMapPosition;

        private IClientSceneObject sceneObject;

        private BoundsUshort worldBounds;

        public WorldMapController(
            PanningPanel panningPanel,
            ViewModelControlWorldMap viewModelControlWorldMap,
            bool isPlayerMarkDisplayed,
            bool isCurrentCameraViewDisplayed,
            bool isListeningToInput,
            int paddingChunks,
            ControlTemplate customControlTemplatePlayerMark = null)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken = CancellationTokenSource
                                     .CreateLinkedTokenSource(this.cancellationTokenSource.Token, Api.CancellationToken)
                                     .Token;

            this.panningPanel = panningPanel;
            this.viewModelControlWorldMap = viewModelControlWorldMap;
            this.isPlayerMarkDisplayed = isPlayerMarkDisplayed;
            this.isCurrentCameraViewDisplayed = isCurrentCameraViewDisplayed;
            this.isListeningToInput = isListeningToInput;
            this.paddingChunks = paddingChunks;
            this.customControlTemplatePlayerMark = customControlTemplatePlayerMark;

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
                Fill = new SolidColorBrush(Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF)),
                //Stroke = new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF)),
                //StrokeThickness = 1.5,
                UseLayoutRounding = false
            };

            Panel.SetZIndex(controlCurrentCameraView, 10);
            if (this.isCurrentCameraViewDisplayed)
            {
                this.canvasMapChildren.Add(controlCurrentCameraView);
            }

            this.sceneObject.AddComponent<ClientComponentWorldMapCurrentCameraViewUpdater>()
                .Setup(this,
                       controlCurrentCameraView,
                       WorldMapTexturesProvider.WorldTileTextureSize);

            panningPanel.CallbackGetSliderZoomCanvasPosition =
                () => this.isAutocenterOnPlayer
                          ? this.componentCurrentCharacterUpdater.CanvasPosition
                          : (Vector2D?)null;

            // setup events
            World.WorldChunkAddedOrUpdated += this.WorldChunkAddedOrUpdatedHandler;
            World.WorldBoundsChanged += this.WorldBoundsChangedHandler;
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

                if (this.isActive
                    && this.isListeningToInput)
                {
                    this.clintInputContext =
                        ClientInputContext.Start(nameof(WorldMapController))
                                          .HandleAll(() =>
                                                     {
                                                         if (Api.Client.Input.IsKeyDown(InputKey.Space))
                                                         {
                                                             this.CenterMapOnPlayerCharacter(
                                                                 resetZoomIfBelowThreshold: this.isListeningToInput);
                                                         }
                                                     });
                }
            }
        }

        public Vector2Ushort PointedMapPositionWithOffset
            => this.PointedMapPositionWithoutOffset - this.worldBounds.Offset;

        public Vector2Ushort PointedMapPositionWithoutOffset
            => this.ScreenToWorldPosition();

        public static IEnumerable<Vector2Ushort> OrderChunksByProximityToPlayer(
            IEnumerable<Vector2Ushort> worldChunksAvailable)
        {
            var playerCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            if (playerCharacter is null)
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

        public void CenterMapOnPlayerCharacter(bool resetZoomIfBelowThreshold)
        {
            this.isAutocenterOnPlayer = true;
            var canvasPosition = this.componentCurrentCharacterUpdater.CanvasPosition;
            this.lastPlayerCanvasPosition = canvasPosition;
            //Api.Logger.Write("Centering on player at canvas position: " + canvasPosition);

            if (resetZoomIfBelowThreshold
                && this.panningPanel.CurrentTargetZoom < 0.5)
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
            World.WorldChunkAddedOrUpdated -= this.WorldChunkAddedOrUpdatedHandler;
            World.WorldBoundsChanged -= this.WorldBoundsChangedHandler;
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
            var x = canvasPosition.X / WorldMapTexturesProvider.WorldTileTextureSize;
            var y = canvasPosition.Y / WorldMapTexturesProvider.WorldTileTextureSize;

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

            return (x * WorldMapTexturesProvider.WorldTileTextureSize,
                    y * WorldMapTexturesProvider.WorldTileTextureSize);
        }

        protected void AddOrRefreshChunk(Vector2Ushort chunkStartPosition)
        {
            var sectorPosition = this.CalculateSectorPosition(chunkStartPosition);
            if (!this.canvasMapSectorControls.TryGetValue(sectorPosition, out var sector))
            {
                var sectorCanvas = new Canvas();
                var sectorVisualPosition = this.WorldToCanvasPosition(sectorPosition.ToVector2D());
                Canvas.SetLeft(sectorCanvas, sectorVisualPosition.X);
                Canvas.SetTop(sectorCanvas, sectorVisualPosition.Y);

                sector = new SectorControl(sectorCanvas, sectorPosition);
                this.canvasMapChildren.Add(sectorCanvas);
                this.canvasMapSectorControls.Add(sectorPosition, sector);
            }

            var checksum = World.GetWorldChunkChecksum(chunkStartPosition);
            if (sector.TryGetTileRectangle(chunkStartPosition, out var tileRectangle))
            {
                // already added - need to refresh the texture
                Load(tileRectangle, chunkStartPosition, checksum, this.cancellationToken);
                return;
            }

            // need to create a new rectangle and add it to Canvas
            // to fix NoesisGUI rendering issue (1 px holes between rectangles)
            const double squareSize = WorldMapTexturesProvider.WorldChunkMapTextureSize + 1;
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

        protected Vector2Ushort CalculateSectorPosition(in Vector2Ushort position)
        {
            return new Vector2Ushort((ushort)((position.X / SectorSize) * SectorSize),
                                     (ushort)((position.Y / SectorSize) * SectorSize));
        }

        protected void MarkDirty()
        {
            this.lastCurrentMapPosition = Vector2Ushort.Max;
            this.lastPlayerCanvasPosition = null;
            this.panningPanel.Refresh();
        }

        protected virtual void RefreshQueue(Vector2Ushort currentWorldPosition)
        {
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
                || WorldMapTexturesProvider.IsBusy)
            {
                return;
            }

            // process queued chunks
            var index = 0;
            do
            {
                var chunkStartPosition = this.queueChunks[index];
                this.AddOrRefreshChunk(chunkStartPosition);
                index++;

                if (WorldMapTexturesProvider.IsBusy)
                {
                    break;
                }
            }
            while (index < this.queueChunks.Count);

            // remove processed chunks
            this.queueChunks.RemoveRange(0, index);
        }

        private static async void Load(
            Rectangle tileRectangle,
            Vector2Ushort chunkStartPosition,
            uint checksum,
            CancellationToken cancellationToken)
        {
            var imageBrush = await WorldMapTexturesProvider.LoadMapChunkImageBrush(chunkStartPosition,
                                 checksum,
                                 cancellationToken);
            if (imageBrush is null)
            {
                // cancelled
                return;
            }

            // verify if this is an actual chunk
            var currentChecksum = World.GetWorldChunkChecksum(chunkStartPosition);
            if (currentChecksum == checksum)
            {
                tileRectangle.Fill = imageBrush;
            }
        }

        private double CalculateExtraControlsScale()
        {
            return 1 / this.panningPanel.CurrentAnimatedZoom;
        }

        private Vector2D CanvasToWorldPosition(Vector2D canvasPosition)
        {
            var x = canvasPosition.X / WorldMapTexturesProvider.WorldTileTextureSize;
            var y = canvasPosition.Y / WorldMapTexturesProvider.WorldTileTextureSize;
            y -= this.worldBounds.Size.Y;

            x += this.worldBounds.Offset.X;
            y = this.worldBounds.Offset.Y - y;

            return (x, y);
        }

        private void CreateCurrentCharacterControl()
        {
            Control controlCurrentCharacter = this.customControlTemplatePlayerMark is not null
                                                  ? new Control() { Template = this.customControlTemplatePlayerMark }
                                                  : new WorldMapMarkCurrentCharacter();
            Panel.SetZIndex(controlCurrentCharacter, 10);
            if (this.isPlayerMarkDisplayed)
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
            this.worldBounds = World.WorldBounds;
            this.panningPanel.PanningWidth = this.worldBounds.Size.X * WorldMapTexturesProvider.WorldTileTextureSize;
            this.panningPanel.PanningHeight = this.worldBounds.Size.Y * WorldMapTexturesProvider.WorldTileTextureSize;

            foreach (var sectorControl in this.canvasMapSectorControls.Values)
            {
                this.canvasMapChildren.Remove(sectorControl.Canvas);
            }

            this.canvasMapSectorControls.Clear();

            this.queueChunks.Clear();
            this.newChunksHashSet.Clear();
            this.queueChunks.AddRange(World.AvailableWorldChunks);

            this.isVisibleMapBoundsDirty = true;
            this.UpdateMapBoundsIfDirty();

            //Api.Logger.WriteDev($"Map init: total world chunks available count: {World.WorldChunksAvailable.Count}");
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

            if (Api.Client.CurrentGame.ConnectionState != ConnectionState.Connected
                || Api.Client.Characters.CurrentPlayerCharacter is null)
            {
                // not connected or not ready - don't update the map
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

                if (this.isListeningToInput)
                {
                    this.viewModelControlWorldMap.PointedPositionText = pointedMapPosition.ToString();

                    var protoTile = this.panningPanel.IsMouseOver
                                        ? Api.Client.World.GetTile(this.PointedMapPositionWithoutOffset).ProtoTile
                                        : null;
                    if (protoTile is null
                        || protoTile is TilePlaceholder)
                    {
                        this.viewModelControlWorldMap.PointedPositionBiomeName = string.Empty;
                    }
                    else
                    {
                        this.viewModelControlWorldMap.PointedPositionBiomeName = protoTile.Name;
                    }
                }
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

                this.CenterMapOnPlayerCharacter(resetZoomIfBelowThreshold: this.isListeningToInput);
            }

            AutoCenterMapOnPlayerCharacter();

            if (WorldMapTexturesProvider.IsBusy)
            {
                return;
            }

            this.RefreshQueue(currentWorldPosition);
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

            if (true)
            {
                boundsVisible = this.worldBounds;
            }
            else // TODO: temporary disabled
                // we need a better algorithm to calculate the max "observable" world bounds with proper padding
                // otherwise the map is moving when new world chunks are explored and bounds extended
            {
                var allChunks = World.AvailableWorldChunks.ToList();
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
                var padding = this.paddingChunks * ScriptingConstants.WorldChunkSize;
                var minX = (ushort)Math.Max(0, boundsVisible.MinX - padding);
                var minY = (ushort)Math.Max(0, boundsVisible.MinY - padding);
                var maxX = (ushort)Math.Min(ushort.MaxValue, boundsVisible.MaxX + padding);
                var maxY = (ushort)Math.Min(ushort.MaxValue, boundsVisible.MaxY + padding);
                boundsVisible = new BoundsUshort(minX, minY, maxX, maxY);
            }

            // don't allow scale closer than that
            this.panningPanel.MaxZoom = 1;

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

            var totalWorldChunksCount = World.DiscoverableWorldChunksTotalCount;
            var percent = World.DiscoveredWorldChunksCount / (double)totalWorldChunksCount;
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
            this.isVisibleMapBoundsDirty = true;
        }

        protected class SectorControl
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

            public Dictionary<Vector2Ushort, Rectangle>.KeyCollection Visualizers
                => this.chunkVisualizers.Keys;

            public void AddTileRectangle(in Vector2Ushort chunkStartPosition, Rectangle rectangle)
            {
                this.chunkVisualizers.Add(chunkStartPosition, rectangle);
                this.canvasChildren.Add(rectangle);
            }

            public void RemoveTileRectangle(in Vector2Ushort chunkStartPosition)
            {
                if (this.chunkVisualizers.TryGetValue(chunkStartPosition, out var rectangle))
                {
                    this.chunkVisualizers.Remove(chunkStartPosition);
                    this.canvasChildren.Remove(rectangle);
                }
                else
                {
                    Api.Logger.Error("Chunk not found: " + chunkStartPosition);
                }
            }

            public bool TryGetTileRectangle(in Vector2Ushort chunkStartPosition, out Rectangle rectangle)
            {
                return this.chunkVisualizers.TryGetValue(chunkStartPosition, out rectangle);
            }
        }
    }
}