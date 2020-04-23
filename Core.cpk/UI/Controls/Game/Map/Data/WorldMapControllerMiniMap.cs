namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class WorldMapControllerMiniMap : WorldMapController
    {
        public Vector2Ushort mapAreaSize;

        private readonly HashSet<Vector2Ushort> currentObservedChunks
            = new HashSet<Vector2Ushort>();

        public WorldMapControllerMiniMap(
            PanningPanel panningPanel,
            ViewModelControlWorldMap viewModelControlWorldMap,
            bool isPlayerMarkDisplayed,
            bool isListeningToInput,
            int paddingChunks,
            Vector2Ushort mapAreaSize)
            : base(panningPanel,
                   viewModelControlWorldMap,
                   isPlayerMarkDisplayed,
                   isListeningToInput,
                   paddingChunks)
        {
            this.mapAreaSize = mapAreaSize;
        }

        public Vector2Ushort MapAreaSize
        {
            get => this.mapAreaSize;
            set
            {
                this.mapAreaSize = value;
                this.MarkDirty();
            }
        }

        protected override void RefreshQueue(Vector2Ushort currentWorldPosition)
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

            // TODO: unload now invisible map sectors/chunks?
        }

        private BoundsUshort CalculateMapBounds(Vector2Ushort currentWorldPosition)
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

        private void UpdateObservedChunks(Vector2Ushort currentWorldPosition)
        {
            var bounds = this.CalculateMapBounds(currentWorldPosition);

            this.currentObservedChunks.Clear();

            using var tempWorldChunks = Api.Shared.GetTempList<Vector2Ushort>();
            ScriptingWorldChunkHelper.CalculateRequiredWorldChunks(bounds, tempWorldChunks);

            foreach (var chunkStartPosition in tempWorldChunks.AsList())
            {
                this.currentObservedChunks.Add(chunkStartPosition);

                var sectorPosition = this.CalculateSectorPosition(chunkStartPosition);
                if (this.canvasMapSectorControls.TryGetValue(sectorPosition, out var sector)
                    && sector.TryGetTileRectangle(chunkStartPosition, out var tileRectangle))
                {
                    continue;
                }

                if (!World.IsWorldChunkAvailable(chunkStartPosition))
                {
                    continue;
                }

                this.queueChunks.AddIfNotContains(chunkStartPosition);
                //Api.Logger.Dev("Started observing chunk: " + chunkStartPosition);
            }

            // find chunks which are no longer visible
            tempWorldChunks.Clear();

            foreach (var mapSectorControl in this.canvasMapSectorControls)
            {
                var sectorControl = mapSectorControl.Value;
                foreach (var chunkStartPosition in sectorControl.Visualizers)
                {
                    if (this.currentObservedChunks.Contains(chunkStartPosition))
                    {
                        continue;
                    }

                    // need to remove this chunk as it's no longer visible
                    tempWorldChunks.Add(chunkStartPosition);
                }
            }

            if (tempWorldChunks.Count == 0)
            {
                return;
            }

            // remove chunks which are no longer visible
            foreach (var chunkStartPosition in tempWorldChunks.AsList())
            {
                var sectorPosition = this.CalculateSectorPosition(chunkStartPosition);
                var sector = this.canvasMapSectorControls[sectorPosition];
                sector.RemoveTileRectangle(chunkStartPosition);
                //Api.Logger.Dev("Removed chunk: " + chunkStartPosition);
            }
        }
    }
}