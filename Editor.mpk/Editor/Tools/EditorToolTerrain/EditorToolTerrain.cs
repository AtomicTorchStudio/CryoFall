namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EditorToolTerrain : BaseEditorTool<EditorToolTerrainItem>
    {
        private byte lastTargetHeight;

        private IProtoTile lastTargetProto;

        private ViewModelEditorToolTerrainSettings settings;

        public override string Name => "Terrain editor tool";

        public override int Order => 10;

        public override BaseEditorActiveTool Activate(EditorToolTerrainItem item)
        {
            var tool = new EditorActiveToolTileBrush(
                onSelected: (tilePositions, isRepeat)
                                => this.ClientPlaceAt(tilePositions, item.ProtoTile, isRepeat));

            this.SetupActiveTool(tool);
            return tool;
        }

        public override FrameworkElement CreateSettingsControl()
        {
            var control = new EditorToolTerrainSettings();
            if (this.settings is null)
            {
                this.settings = new ViewModelEditorToolTerrainSettings();
                this.settings.BrushSettingsChanged += this.BrushSettingsChangedHandler;
            }

            control.Setup(this.settings);
            return control;
        }

        protected override void SetupFilters(List<EditorToolItemFilter<EditorToolTerrainItem>> filters)
        {
            // no filters
        }

        protected override void SetupItems(List<EditorToolTerrainItem> items)
        {
            var groundProtos = Api.FindProtoEntities<IProtoTile>();

            items.Add(new EditorToolTerrainItem(null));

            foreach (var protoTile in groundProtos
                                      .Where(t => t.EditorIconTexture is not null)
                                      .OrderBy(t => t.Kind)
                                      .ThenBy(t => t.ShortId))
            {
                if (protoTile is IProtoTileWater protoTileWater
                    && ReferenceEquals(protoTileWater.BridgeProtoTile, protoTile))
                {
                    // ignore bridges
                    continue;
                }
                
                items.Add(new EditorToolTerrainItem(protoTile));
            }
        }

        private static bool IsValidHeightFotTile(Tile tile, byte newHeight)
        {
            return tile.EightNeighborTiles.All(t => Math.Abs(t.Height - newHeight) <= 1);
        }

        private void BrushSettingsChangedHandler()
        {
            this.SetupActiveTool((EditorActiveToolTileBrush)EditorActiveToolManager.ActiveTool);
        }

        private byte CalculateNewTileHeight(Tile tile, TerrainHeightMode heightMode)
        {
            byte newHeight = 0;

            switch (heightMode)
            {
                case TerrainHeightMode.Keep:
                default:
                    return tile.Height;

                case TerrainHeightMode.Increase:
                    if (tile.Height >= 64)
                    {
                        // cannot increase height
                        break;
                    }

                    newHeight = (byte)(tile.Height + 1);
                    break;

                case TerrainHeightMode.Decrease:
                    if (tile.Height == 0)
                    {
                        // cannot decrease height
                        break;
                    }

                    newHeight = (byte)(tile.Height - 1);
                    break;
            }

            // check if neighbor tiles allows the height increase/decrease of the tile
            if (IsValidHeightFotTile(tile, newHeight))
            {
                return newHeight;
            }

            return tile.Height;
        }

        private void ClientPlaceAt(List<Vector2Ushort> tilePositions, IProtoTile selectedProtoTile, bool isRepeat)
        {
            var terrainHeightMode = this.settings.SelectedHeightMode.Value;
            var isAllowTileKindChange = this.settings.IsAllowTileKindChange;
            var isAllowTileProtoChangeOnlyOnTheSameHeight = this.settings.IsAllowTileProtoChangeOnlyOnTheSameHeight;
            var isApplyOnlyOnTheSameTileProto = this.settings.IsApplyOnlyOnTheSameTileProto;

            if (this.settings.IsFillMode)
            {
                if (isRepeat)
                {
                    // fill doesn't support repeat
                    return;
                }

                tilePositions = EditorTileHelper.GatherAllTilePositionsOfTheSameProtoTile(
                    tilePositions[0],
                    onlyOnTheSameHeight: isAllowTileProtoChangeOnlyOnTheSameHeight,
                    ignoreCliffsAndSlopes: false);

                // don't change the tile heights in the fill mode
                terrainHeightMode = TerrainHeightMode.Keep;
            }

            var worldService = Client.World;

            byte targetHeight = 0;
            IProtoTile targetProto = null;

            if (isRepeat)
            {
                // use target height from previous iteration
                targetHeight = this.lastTargetHeight;
                targetProto = this.lastTargetProto;
            }
            else
            {
                if (isApplyOnlyOnTheSameTileProto)
                {
                    targetProto = EditorTileHelper.CalculateMostFrequentTileProto(tilePositions);
                }

                switch (terrainHeightMode)
                {
                    case TerrainHeightMode.Keep:
                    case TerrainHeightMode.Flatten:
                        // calculate average height for all the tiles
                        targetHeight = EditorTileHelper.CalculateAverageHeight(tilePositions);
                        break;

                    case TerrainHeightMode.Increase:
                        targetHeight = byte.MaxValue;
                        goto case TerrainHeightMode.Decrease;

                    case TerrainHeightMode.Decrease:
                        // calculate target height
                        foreach (var tilePosition in tilePositions)
                        {
                            var tile = worldService.GetTile(tilePosition);
                            var calculatedNewTileHeight = this.CalculateNewTileHeight(tile, terrainHeightMode);

                            if (terrainHeightMode == TerrainHeightMode.Increase
                                && calculatedNewTileHeight < targetHeight
                                || terrainHeightMode == TerrainHeightMode.Decrease
                                && calculatedNewTileHeight > targetHeight)
                            {
                                targetHeight = calculatedNewTileHeight;
                            }
                        }

                        break;
                }
            }

            this.lastTargetHeight = targetHeight;
            this.lastTargetProto = targetProto;

            var tilesToModify = new List<List<TileModifyRequest>>();
            var tilesToRevert = new List<List<TileModifyRequest>>();

            // separate on groups of world chunk size
            var worldChunkSize = 10 * ScriptingConstants.WorldChunkSize;
            var groups = tilePositions
                         .GroupBy(g => new Vector2Ushort((ushort)(g.X / worldChunkSize),
                                                         (ushort)(g.Y / worldChunkSize)))
                         .ToList();

            // gather tile modifications for each group
            foreach (var group in groups)
            {
                var groupTilesToModify = new List<TileModifyRequest>();
                var groupTilesToRevert = new List<TileModifyRequest>();

                foreach (var tilePosition in group)
                {
                    var tile = worldService.GetTile(tilePosition);
                    var previousProtoTile = tile.ProtoTile;
                    var previousTileHeight = tile.Height;
                    var previousIsSlope = tile.IsSlope;

                    var newProtoTile = selectedProtoTile ?? previousProtoTile;
                    var newIsSlope = tile.IsSlope;

                    if (isApplyOnlyOnTheSameTileProto
                        && previousProtoTile != targetProto)
                    {
                        continue;
                    }

                    if (isAllowTileProtoChangeOnlyOnTheSameHeight
                        && this.lastTargetHeight != previousTileHeight)
                    {
                        continue;
                    }

                    if (!isAllowTileKindChange
                        && previousProtoTile.Kind != newProtoTile.Kind
                        && previousProtoTile.Kind != TileKind.Placeholder)
                    {
                        continue;
                    }

                    var newTileHeight = previousTileHeight;
                    if (terrainHeightMode == TerrainHeightMode.Flatten)
                    {
                        if (IsValidHeightFotTile(tile, targetHeight))
                        {
                            // can set tile height to target height
                            newTileHeight = targetHeight;
                        }
                    }
                    else if (terrainHeightMode == TerrainHeightMode.Increase
                             || terrainHeightMode == TerrainHeightMode.Decrease)
                    {
                        newTileHeight = this.CalculateNewTileHeight(tile, terrainHeightMode);
                        if (newTileHeight != targetHeight)
                        {
                            // cannot change tile height
                            newTileHeight = previousTileHeight;
                        }
                    }

                    if (previousProtoTile == newProtoTile
                        && newTileHeight == previousTileHeight
                        && newIsSlope == previousIsSlope)
                    {
                        // nothing to change - the tile is already as desired
                        continue;
                    }

                    groupTilesToModify.Add(new TileModifyRequest(tilePosition,
                                                                 newProtoTile.SessionIndex,
                                                                 newTileHeight,
                                                                 newIsSlope));
                    groupTilesToRevert.Add(new TileModifyRequest(tilePosition,
                                                                 previousProtoTile.SessionIndex,
                                                                 previousTileHeight,
                                                                 previousIsSlope));
                }

                if (groupTilesToModify.Count == 0)
                {
                    // nothing to modify in this group
                    continue;
                }

                tilesToModify.Add(groupTilesToModify);
                tilesToRevert.Add(groupTilesToRevert);
            }

            if (tilesToModify.Count == 0)
            {
                // nothing to modify
                return;
            }

            EditorClientSystem.DoAction(
                "Modify terrain tiles",
                onDo: () => tilesToModify.ForEach(
                          chunk => this.CallServer(_ => _.ServerRemote_PlaceAt(chunk))),
                onUndo: () => tilesToRevert.ForEach(
                            chunk => this.CallServer(_ => _.ServerRemote_PlaceAt(chunk))));
        }

        [RemoteCallSettings(DeliveryMode.Default, clientMaxSendQueueSize: byte.MaxValue)]
        private void ServerRemote_PlaceAt(IList<TileModifyRequest> modifyRequests)
        {
            if (modifyRequests.Count == 0)
            {
                throw new Exception("Incorrect modify request - at least one tile is required");
            }

            //Logger.Write("Modify terrain at " + modifyRequests[0].TilePosition);

            var worldService = Server.World;
            foreach (var request in modifyRequests)
            {
                var protoTile = worldService.GetProtoTileBySessionIndex(request.ProtoTileSessionIndex);
                worldService.SetTileData(
                    request.TilePosition,
                    protoTile,
                    tileHeight: request.SetHeight,
                    isSlope: request.SeIsSlope,
                    isCliff: false);
            }

            worldService.FixMapTilesRecentlyModified();
        }

        private void SetupActiveTool(EditorActiveToolTileBrush tool)
        {
            var brushSize = this.settings.SelectedBrushSize;
            var brush = this.settings.SelectedBrushShape.Value;
            tool.SetBrush(brush, brushSize);
        }

        private struct TileModifyRequest : IRemoteCallParameter
        {
            public readonly byte ProtoTileSessionIndex;

            public readonly bool SeIsSlope;

            public readonly byte SetHeight;

            public readonly Vector2Ushort TilePosition;

            public TileModifyRequest(
                Vector2Ushort tilePosition,
                byte protoTileSessionIndex,
                byte setHeight,
                bool seIsSlope)
            {
                this.TilePosition = tilePosition;
                this.SetHeight = setHeight;
                this.SeIsSlope = seIsSlope;
                this.ProtoTileSessionIndex = protoTileSessionIndex;
            }
        }
    }
}