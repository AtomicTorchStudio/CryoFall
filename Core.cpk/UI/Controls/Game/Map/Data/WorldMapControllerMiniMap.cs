namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class WorldMapControllerMiniMap : WorldMapController
    {
        /*private readonly HashSet<Vector2Ushort> currentObservedSectors
            = new HashSet<Vector2Ushort>();*/

        private Vector2Ushort mapAreaSize;

        public WorldMapControllerMiniMap(
            PanningPanel panningPanel,
            ViewModelControlWorldMap viewModelControlWorldMap,
            bool isPlayerMarkDisplayed,
            bool isCurrentCameraViewDisplayed,
            bool isListeningToInput,
            int paddingChunks,
            Vector2Ushort mapAreaSize,
            WorldMapSectorProvider sectorProvider,
            ControlTemplate customControlTemplatePlayerMark = null)
            : base(panningPanel,
                   sectorProvider,
                   viewModelControlWorldMap,
                   isPlayerMarkDisplayed,
                   isCurrentCameraViewDisplayed,
                   isListeningToInput,
                   paddingChunks,
                   customControlTemplatePlayerMark)
        {
            this.mapAreaSize = mapAreaSize;
        }

        public override bool IsCoordinateGridOverlayEnabled => false;

        /*protected override void RefreshQueue(Vector2Ushort currentWorldPosition)
        {
            this.newChunksHashSet.Clear();
            this.queueChunks.Clear();

            this.UpdateObservedChunks(currentWorldPosition);

            if (this.queueChunks.Count == 0)
            {
                return;
            }

            // Please note: no busy state checking as it interferes with base map loading
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
        }*/

        /*private BoundsUshort CalculateMapBounds(Vector2Ushort currentWorldPosition)
        {
            var halfSizeX = this.mapAreaSize.X / 2;
            var halfSizeY = this.mapAreaSize.Y / 2;

            var startPositionX = currentWorldPosition.X - halfSizeX;
            var startPositionY = currentWorldPosition.Y - halfSizeY;
            var endPositionX = currentWorldPosition.X + halfSizeX;
            var endPositionY = currentWorldPosition.Y + halfSizeY;

            return new BoundsUshort((ushort)MathHelper.Clamp(startPositionX, 0, ushort.MaxValue),
                                    (ushort)MathHelper.Clamp(startPositionY, 0, ushort.MaxValue),
                                    (ushort)MathHelper.Clamp(endPositionX,   0, ushort.MaxValue),
                                    (ushort)MathHelper.Clamp(endPositionY,   0, ushort.MaxValue));
        }
        */

        /*private void UpdateObservedChunks(Vector2Ushort currentWorldPosition)
        {
            var bounds = this.CalculateMapBounds(currentWorldPosition);

            this.currentObservedSectors.Clear();

            using var tempSectorsList = Api.Shared.GetTempList<Vector2Ushort>();
            WorldMapSectorHelper.CalculateRequiredSectors(bounds, tempSectorsList);

            foreach (var sectorPosition in tempSectorsList.AsList())
            {
                this.currentObservedSectors.Add(sectorPosition);
                if (this.canvasMapSectors.TryGetValue(sectorPosition, out var sector))
                {
                    // sector already loaded
                    continue;
                }

                Api.Logger.Dev("[Minimap] Started observing sector: " + sectorPosition);

                for (var y = sectorPosition.Y;
                     y < sectorPosition.Y + WorldMapSectorProvider.SectorWorldSize;
                     y += ScriptingConstants.WorldChunkSize)
                for (var x = sectorPosition.X;
                     x < sectorPosition.X + WorldMapSectorProvider.SectorWorldSize;
                     x += ScriptingConstants.WorldChunkSize)
                {
                    var chunkPosition = (x, y);
                    if (World.IsWorldChunkAvailable(chunkPosition))
                    {
                        this.queueChunks.AddIfNotContains(chunkPosition);
                        Api.Logger.Dev("[Minimap] Observing world chunk: " + chunkPosition);
                    }
                }
            }

            // find sectors which are no longer visible
            tempSectorsList.Clear();
            foreach (var mapSectorControl in this.canvasMapSectors)
            {
                var sectorControl = mapSectorControl.Value;
                var sectorPosition = sectorControl.SectorWorldPosition;
                if (this.currentObservedSectors.Contains(sectorPosition))
                {
                    continue;
                }

                // need to remove this chunk as it's no longer visible
                tempSectorsList.Add(sectorPosition);
            }

            if (tempSectorsList.Count == 0)
            {
                return;
            }

            // remove sectors which are no longer visible
            foreach (var chunkStartPosition in tempSectorsList.AsList())
            {
                var sectorPosition = WorldMapSectorHelper.CalculateSectorStartPosition(chunkStartPosition);
                var sector = this.canvasMapSectors[sectorPosition];
                this.canvasMapChildren.Remove(sector.SectorRectangle);
                this.canvasMapSectors.Remove(sectorPosition);
                Api.Logger.Dev("[Minimap] No longer observing sector: " + sectorPosition);
            }
        }*/

        public Vector2Ushort MapAreaSize
        {
            get => this.mapAreaSize;
            set
            {
                if (this.mapAreaSize == value)
                {
                    return;
                }

                this.mapAreaSize = value;
                this.MarkDirty();
            }
        }
    }
}