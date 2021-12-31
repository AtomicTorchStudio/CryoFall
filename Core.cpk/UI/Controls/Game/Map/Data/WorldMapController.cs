namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
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
        private const double CoordinateLettersMarginBase = 2.5;

        protected static readonly IWorldClientService World = Api.Client.World;

        private static readonly Brush BrushCoordinateGridOverlay
            = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));

        private static readonly double CoordinateGridLineThickess
            = WorldMapSectorProvider.WorldTileTextureSize / 6.0;

        public Action<Vector2D> MapClickCallback;

        protected readonly UIElementCollection canvasMapChildren;

        protected readonly Dictionary<Vector2Ushort, Sector> canvasMapSectors;

        protected readonly HashSet<Vector2Ushort> newChunksHashSet = new();

        protected readonly List<Vector2Ushort> queueChunks = new(capacity: 10000);

        private readonly Canvas canvasMap;

        private readonly List<Line> controlsCoordinateGridLines = new();

        private readonly List<FrameworkElement> controlsCoordinateLettersAll = new();

        private readonly List<FrameworkElement> controlsCoordinateLettersBottomSide = new();

        private readonly List<FrameworkElement> controlsCoordinateLettersLeftSide = new();

        private readonly List<FrameworkElement> controlsCoordinateLettersRightSide = new();

        private readonly List<FrameworkElement> controlsCoordinateLettersTopSide = new();

        private readonly FrameworkElement coordinateLettersMarginSource = new();

        private readonly ScaleTransform coordinateLettersScaleTransform = new();

        private readonly List<UIElement> extraControls = new();

        private readonly bool isListeningToInput;

        private readonly bool isPlayerMarkDisplayed;

        private readonly int paddingChunks;

        private readonly PanningPanel panningPanel;

        private readonly ViewModelControlWorldMap viewModelControlWorldMap;

        private ClientInputContext clintInputContext;

        private ClientComponentWorldMapCurrentCharacterUpdater componentCurrentCharacterUpdater;

        private readonly ControlTemplate customControlTemplatePlayerMark;

        private bool isActive;

        private bool isAutocenterOnPlayer = true;

        private readonly bool isCurrentCameraViewDisplayed;

        private bool isDisposed;

        private bool isVisibleMapBoundsDirty = true;

        private Vector2D lastCoordinateLettersCanvasPositionBottomRight;

        private Vector2Ushort lastCurrentWorldPosition;

        private double lastExtraControlsScale = 1;

        private Vector2D? lastPlayerCanvasPosition;

        private Vector2Ushort lastPointedMapPositionAbsolute;

        private IClientSceneObject sceneObject;

        private readonly WorldMapSectorProvider sectorProvider;

        private BoundsUshort worldBounds;

        public WorldMapController(
            PanningPanel panningPanel,
            WorldMapSectorProvider sectorProvider,
            ViewModelControlWorldMap viewModelControlWorldMap,
            bool isPlayerMarkDisplayed,
            bool isCurrentCameraViewDisplayed,
            bool isListeningToInput,
            int paddingChunks,
            ControlTemplate customControlTemplatePlayerMark = null)
        {
            sectorProvider.UsageCounter++;
            this.panningPanel = panningPanel;
            this.viewModelControlWorldMap = viewModelControlWorldMap;
            this.isPlayerMarkDisplayed = isPlayerMarkDisplayed;
            this.isCurrentCameraViewDisplayed = isCurrentCameraViewDisplayed;
            this.isListeningToInput = isListeningToInput;
            this.paddingChunks = paddingChunks;
            this.sectorProvider = sectorProvider;
            this.customControlTemplatePlayerMark = customControlTemplatePlayerMark;

            this.canvasMap = new Canvas();
            this.canvasMapChildren = this.canvasMap.Children;
            this.canvasMapSectors = new Dictionary<Vector2Ushort, Sector>();

            this.panningPanel.Items.Clear();
            this.panningPanel.Items.Add(this.canvasMap);
            this.panningPanel.Items.Add(this.coordinateLettersMarginSource);

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
                       WorldMapSectorProvider.WorldTileTextureSize);

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

        public virtual bool IsCoordinateGridOverlayEnabled => true;

        public Vector2Ushort PointedMapWorldPositionAbsolute
            => this.ScreenToWorldPosition();

        public Vector2Ushort PointedMapWorldPositionRelative
            => this.PointedMapWorldPositionAbsolute - this.worldBounds.Offset;

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
            //Api.Logger.Dev("Centering on player at canvas position: " + canvasPosition);

            if (resetZoomIfBelowThreshold)
            {
                /*&& this.panningPanel.CurrentTargetZoom < this.panningPanel.DefaultZoom)*/
                this.panningPanel.SetZoom(this.panningPanel.DefaultZoom);
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

            this.sectorProvider.UsageCounter--;

            this.sceneObject.Destroy();
            this.queueChunks.Clear();
            this.newChunksHashSet.Clear();
            World.WorldChunkAddedOrUpdated -= this.WorldChunkAddedOrUpdatedHandler;
            World.WorldBoundsChanged -= this.WorldBoundsChangedHandler;
            this.panningPanel.MouseHold -= this.PanningPanelMouseHoldHandler;
            this.panningPanel.MouseLeftButtonClick -= this.PanningPanelMouseLeftButtonClickHandler;
            ClientUpdateHelper.UpdateCallback -= this.Update;

            foreach (var sector in this.canvasMapSectors.Values)
            {
                this.canvasMapChildren.Remove(sector.SectorRectangle);
            }

            this.canvasMapSectors.Clear();
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
            var x = canvasPosition.X / WorldMapSectorProvider.WorldTileTextureSize;
            var y = canvasPosition.Y / WorldMapSectorProvider.WorldTileTextureSize;

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

            return (x * WorldMapSectorProvider.WorldTileTextureSize,
                    y * WorldMapSectorProvider.WorldTileTextureSize);
        }

        protected void AddOrRefreshChunk(Vector2Ushort chunkStartPosition)
        {
            var sectorPosition = WorldMapSectorHelper.CalculateSectorStartPosition(chunkStartPosition);
            if (!this.canvasMapSectors.TryGetValue(sectorPosition, out var sector))
            {
                var sectorVisualPosition = this.WorldToCanvasPosition(sectorPosition.ToVector2D());
                sector = new Sector(this.sectorProvider, sectorPosition, sectorVisualPosition);
                this.canvasMapChildren.Add(sector.SectorRectangle);
                this.canvasMapSectors.Add(sectorPosition, sector);
            }

            this.sectorProvider.AddOrRefreshChunk(sector.SectorRenderer, chunkStartPosition);
        }

        protected void MarkDirty()
        {
            this.lastCurrentWorldPosition = Vector2Ushort.Max;
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

            if (this.queueChunks.Count == 0)
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
            }
            while (index < this.queueChunks.Count);

            // remove processed chunks
            this.queueChunks.RemoveRange(0, index);
        }

        private void AddCoordinateLetters()
        {
            var (start, end) = this.GetSectorWorldBounds();
            var textBlockCoordinateStyle = Api.Client.UI.GetApplicationResource<Style>("TextBlockCoordinateStyle");
            var sectorSize = WorldMapSectorProvider.SectorWorldSize;
            var sectorCanvasSize = WorldMapSectorProvider.SectorPixelSize;

            var coordinateLetterX = 'A';
            for (var x = start.X; x < end.X; x += sectorSize)
            {
                var letter = (coordinateLetterX++).ToString();
                var controlTop = CreateCoordinateLetterControl(letter,
                                                               HorizontalAlignment.Center,
                                                               VerticalAlignment.Top);
                var canvasPosition = this.WorldToCanvasPosition((x, end.Y + sectorSize));
                Canvas.SetLeft(controlTop, canvasPosition.X);
                // the top coordinate is calculated and set dynamically
                this.controlsCoordinateLettersTopSide.Add(controlTop);

                var controlBottom = CreateCoordinateLetterControl(letter,
                                                                  HorizontalAlignment.Center,
                                                                  VerticalAlignment.Bottom);
                canvasPosition = this.WorldToCanvasPosition((x, start.Y));
                Canvas.SetLeft(controlBottom, canvasPosition.X);
                this.controlsCoordinateLettersBottomSide.Add(controlBottom);
            }

            var coordinateLetterY = 1;
            for (var y = start.Y; y < end.Y; y += sectorSize)
            {
                var letter = (coordinateLetterY++).ToString();
                var controlLeft = CreateCoordinateLetterControl(letter,
                                                                HorizontalAlignment.Left,
                                                                VerticalAlignment.Center);
                var canvasPosition = this.WorldToCanvasPosition((start.X, y + sectorSize));
                Canvas.SetTop(controlLeft, canvasPosition.Y);
                // the left coordinate is calculated and set dynamically
                this.controlsCoordinateLettersLeftSide.Add(controlLeft);

                var controlRight = CreateCoordinateLetterControl(letter,
                                                                 HorizontalAlignment.Right,
                                                                 VerticalAlignment.Center);
                canvasPosition = this.WorldToCanvasPosition((end.X, y + sectorSize));
                Canvas.SetTop(controlRight, canvasPosition.Y);
                this.controlsCoordinateLettersRightSide.Add(controlRight);
            }

            this.lastCoordinateLettersCanvasPositionBottomRight = default;
            this.UpdateCoordinateLetters();

            ContentControl CreateCoordinateLetterControl(
                string text,
                HorizontalAlignment horizontalAlignment,
                VerticalAlignment verticalAlignment)
            {
                var textBlock = new TextBlock()
                {
                    Text = text,
                    Style = textBlockCoordinateStyle,
                    HorizontalAlignment = horizontalAlignment,
                    VerticalAlignment = verticalAlignment,
                    LayoutTransform = this.coordinateLettersScaleTransform
                };

                var control = new ContentControl()
                {
                    Width = sectorCanvasSize,
                    Height = sectorCanvasSize,
                    Content = textBlock
                };

                BindingOperations.SetBinding(
                    textBlock,
                    FrameworkElement.MarginProperty,
                    new Binding(FrameworkElement.MarginProperty.Name)
                    {
                        Source = this.coordinateLettersMarginSource,
                        Mode = BindingMode.OneWay
                    });

                Panel.SetZIndex(control, 100);
                this.canvasMapChildren.Add(control);
                this.controlsCoordinateLettersAll.Add(control);
                return control;
            }
        }

        private double CalculateExtraControlsScale()
        {
            return 1 / this.panningPanel.CurrentAnimatedZoom;
        }

        private Vector2D CanvasToWorldPosition(Vector2D canvasPosition)
        {
            var x = canvasPosition.X / WorldMapSectorProvider.WorldTileTextureSize;
            var y = canvasPosition.Y / WorldMapSectorProvider.WorldTileTextureSize;
            y -= this.worldBounds.Size.Y;

            x += this.worldBounds.Offset.X;
            y = this.worldBounds.Offset.Y - y;

            return (x, y);
        }

        private void CreateCurrentCharacterControl()
        {
            var controlCurrentCharacter = this.customControlTemplatePlayerMark is not null
                                              ? new Control() { Template = this.customControlTemplatePlayerMark }
                                              : new WorldMapMarkCurrentCharacter();
            Panel.SetZIndex(controlCurrentCharacter, 30);
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

        private (Vector2Ushort startPosition, Vector2Ushort endPosition) GetSectorWorldBounds()
        {
            var start = WorldMapSectorHelper.CalculateSectorStartPosition(this.worldBounds.Offset);
            var end = WorldMapSectorHelper.CalculateSectorStartPosition(
                (this.worldBounds.Offset + this.worldBounds.Size).ToVector2Ushort());

            var max = this.worldBounds.Offset + this.worldBounds.Size;

            if ((max.X - end.X) / (double)WorldMapSectorProvider.SectorWorldSize > 0.2)
            {
                // add an extra sector space by X
                end = ((ushort)(end.X + WorldMapSectorProvider.SectorWorldSize), end.Y);
            }

            if ((max.Y - end.Y) / (double)WorldMapSectorProvider.SectorWorldSize > 0.2)
            {
                // add an extra sector space by Y
                end = (end.X, (ushort)(end.Y + WorldMapSectorProvider.SectorWorldSize));
            }

            return (start, end);
        }

        private void InitMap()
        {
            this.worldBounds = World.WorldBounds;
            this.panningPanel.PanningWidth = this.worldBounds.Size.X * WorldMapSectorProvider.WorldTileTextureSize;
            this.panningPanel.PanningHeight = this.worldBounds.Size.Y * WorldMapSectorProvider.WorldTileTextureSize;

            foreach (var sector in this.canvasMapSectors.Values)
            {
                this.canvasMapChildren.Remove(sector.SectorRectangle);
            }

            this.canvasMapSectors.Clear();

            this.queueChunks.Clear();
            this.newChunksHashSet.Clear();
            this.queueChunks.AddRange(World.AvailableWorldChunks);

            this.isVisibleMapBoundsDirty = true;
            this.UpdateMapBoundsIfDirty();

            this.InitMapCoordinateGridOverlay();
            this.UpdateExtraControlsZoom(force: true);

            this.CenterMapOnPlayerCharacter(true);

            //Api.Logger.WriteDev($"Map init: total world chunks available count: {World.WorldChunksAvailable.Count}");
        }

        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        private void InitMapCoordinateGridOverlay()
        {
            foreach (var control in this.controlsCoordinateGridLines)
            {
                this.canvasMapChildren.Remove(control);
            }

            foreach (var control in this.controlsCoordinateLettersAll)
            {
                this.canvasMapChildren.Remove(control);
            }

            this.controlsCoordinateGridLines.Clear();
            this.controlsCoordinateLettersAll.Clear();
            this.controlsCoordinateLettersLeftSide.Clear();
            this.controlsCoordinateLettersRightSide.Clear();
            this.controlsCoordinateLettersTopSide.Clear();
            this.controlsCoordinateLettersBottomSide.Clear();

            if (!this.IsCoordinateGridOverlayEnabled)
            {
                return;
            }

            this.worldBounds = World.WorldBounds;
            var worldTileTextureSize = WorldMapSectorProvider.WorldTileTextureSize;
            var (sectorsStartPosition, sectorsEndPosition) = this.GetSectorWorldBounds();
            var (minX, minY) = this.WorldToCanvasPosition((sectorsStartPosition.X, sectorsEndPosition.Y));
            var (maxX, maxY) = this.WorldToCanvasPosition((sectorsEndPosition.X, sectorsStartPosition.Y));

            var stepSize = WorldMapSectorProvider.SectorWorldSize * worldTileTextureSize;

            for (var y = minY; y <= maxY; y += stepSize)
            for (var x = minX; x <= maxX; x += stepSize)
            {
                var lineVertical = new Line
                {
                    Stroke = BrushCoordinateGridOverlay,
                    X1 = x,
                    X2 = x,
                    Y1 = minY,
                    Y2 = maxY
                };

                var lineHorizontal = new Line
                {
                    Stroke = BrushCoordinateGridOverlay,
                    Y1 = y,
                    Y2 = y,
                    X1 = minX,
                    X2 = maxX
                };

                Panel.SetZIndex(lineHorizontal, 1);
                Panel.SetZIndex(lineVertical,   1);

                this.canvasMapChildren.Add(lineHorizontal);
                this.canvasMapChildren.Add(lineVertical);

                this.controlsCoordinateGridLines.Add(lineHorizontal);
                this.controlsCoordinateGridLines.Add(lineVertical);
            }

            this.UpdateCoordinateGridLines();
            this.AddCoordinateLetters();
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
                this.lastCurrentWorldPosition = Vector2Ushort.Max;
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
            this.UpdateCoordinateLetters();

            this.UpdateMapExplorationProgress();

            var currentWorldPosition = this.componentCurrentCharacterUpdater.WorldPosition.ToVector2Ushort();
            if (currentWorldPosition != this.lastCurrentWorldPosition)
            {
                this.lastCurrentWorldPosition = currentWorldPosition;
                this.viewModelControlWorldMap.CurrentPositionText
                    = WorldMapSectorHelper.FormatWorldPositionWithSectorCoordinate(currentWorldPosition);

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

            var pointedMapPositionAbsolute = this.PointedMapWorldPositionAbsolute;
            if (pointedMapPositionAbsolute != this.lastPointedMapPositionAbsolute)
            {
                this.lastPointedMapPositionAbsolute = pointedMapPositionAbsolute;

                if (this.isListeningToInput)
                {
                    this.viewModelControlWorldMap.PointedPositionText
                        = WorldMapSectorHelper.FormatWorldPositionWithSectorCoordinate(pointedMapPositionAbsolute);

                    var protoTile = this.panningPanel.IsMouseOver
                                        ? Api.Client.World.GetTile(this.PointedMapWorldPositionAbsolute).ProtoTile
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

            this.RefreshQueue(currentWorldPosition);
        }

        private void UpdateCoordinateGridLines()
        {
            if (this.controlsCoordinateGridLines.Count == 0)
            {
                return;
            }

            var scale = this.CalculateExtraControlsScale();
            var strokeThickness = scale * CoordinateGridLineThickess;

            foreach (var line in this.controlsCoordinateGridLines)
            {
                line.StrokeThickness = strokeThickness;
            }
        }

        private void UpdateCoordinateLetters()
        {
            if (this.controlsCoordinateLettersAll.Count == 0)
            {
                return;
            }

            var canvasPositionBottomRight = this.canvasMap.PointFromScreen(
                                                    this.panningPanel.PointToScreen(
                                                        new Point(this.panningPanel.ActualWidth,
                                                                  this.panningPanel.ActualHeight)))
                                                .ToVector2D();

            if (this.lastCoordinateLettersCanvasPositionBottomRight == canvasPositionBottomRight)
            {
                return;
            }

            this.lastCoordinateLettersCanvasPositionBottomRight = canvasPositionBottomRight;

            var canvasPositionTopLeft = this.canvasMap.PointFromScreen(
                                                this.panningPanel.PointToScreen(new Point(0, 0)))
                                            .ToVector2D();

            var (sectorsStartPosition, sectorsEndPosition) = this.GetSectorWorldBounds();
            {
                var worldPositionTopLeft = this.CanvasToWorldPosition(canvasPositionTopLeft);
                worldPositionTopLeft = (Math.Max(sectorsStartPosition.X, worldPositionTopLeft.X),
                                        Math.Min(sectorsEndPosition.Y, worldPositionTopLeft.Y));
                canvasPositionTopLeft = this.WorldToCanvasPosition(worldPositionTopLeft);
            }

            {
                var worldPositionBottomRight = this.CanvasToWorldPosition(canvasPositionBottomRight);

                worldPositionBottomRight = (Math.Min(sectorsEndPosition.X, worldPositionBottomRight.X),
                                            Math.Max(sectorsStartPosition.Y, worldPositionBottomRight.Y));
                worldPositionBottomRight += (-WorldMapSectorProvider.SectorWorldSize,
                                             (WorldMapSectorProvider.SectorWorldSize));
                canvasPositionBottomRight = this.WorldToCanvasPosition(worldPositionBottomRight);
            }

            foreach (var controlLeft in this.controlsCoordinateLettersLeftSide)
            {
                Canvas.SetLeft(controlLeft, canvasPositionTopLeft.X);
            }

            foreach (var controlTop in this.controlsCoordinateLettersTopSide)
            {
                Canvas.SetTop(controlTop, canvasPositionTopLeft.Y);
            }

            foreach (var controlRight in this.controlsCoordinateLettersRightSide)
            {
                Canvas.SetLeft(controlRight, canvasPositionBottomRight.X);
            }

            foreach (var controlBottom in this.controlsCoordinateLettersBottomSide)
            {
                Canvas.SetTop(controlBottom, canvasPositionBottomRight.Y);
            }
        }

        private void UpdateExtraControlsZoom(bool force = false)
        {
            var scale = this.CalculateExtraControlsScale();
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (this.lastExtraControlsScale == scale
                && !force)
            {
                return;
            }

            this.lastExtraControlsScale = scale;
            this.coordinateLettersScaleTransform.ScaleX
                = this.coordinateLettersScaleTransform.ScaleY
                      = scale / 10.0;

            this.coordinateLettersMarginSource.SetCurrentValue(FrameworkElement.MarginProperty,
                                                               new Thickness(CoordinateLettersMarginBase * scale));

            foreach (var extraControl in this.extraControls)
            {
                var transform = extraControl.RenderTransform;
                if (transform is ScaleTransform scaleTransform)
                {
                    scaleTransform.ScaleX = scaleTransform.ScaleY = scale;
                }
            }

            this.UpdateCoordinateGridLines();
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
            this.panningPanel.MaxZoom = 4.0 / WorldMapSectorProvider.WorldTileTextureSize;

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

        protected class Sector
        {
            public readonly FrameworkElement SectorRectangle;

            public readonly WorldMapSectorProvider.SectorRenderer SectorRenderer;

            public readonly Vector2Ushort SectorWorldPosition;

            public Sector(
                WorldMapSectorProvider sectorProvider,
                Vector2Ushort sectorWorldPosition,
                Vector2D sectorVisualPosition)
            {
                this.SectorWorldPosition = sectorWorldPosition;
                this.SectorRenderer = sectorProvider.GetSectorRenderer(sectorWorldPosition);
                var sectorRectangle = new Rectangle()
                {
                    // + 1 is required to prevent a seam from appearing between sector rectangles
                    Width = WorldMapSectorProvider.SectorPixelSize + 1,
                    Height = WorldMapSectorProvider.SectorPixelSize + 1,
                    Fill = Api.Client.UI.GetTextureBrush(this.SectorRenderer.RenderTexture)
                };

                Canvas.SetLeft(sectorRectangle, sectorVisualPosition.X);
                Canvas.SetTop(sectorRectangle, sectorVisualPosition.Y - WorldMapSectorProvider.SectorPixelSize);
                this.SectorRectangle = sectorRectangle;
            }
        }
    }
}