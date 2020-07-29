namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrainNoise
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolGenerator;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EditorToolTerrainNoise : BaseEditorTool
    {
        private ViewModelEditorToolTerrainNoiseSettings settings;

        public override string Name => "Terrain noise";

        public override int Order => 70;

        public override BaseEditorActiveTool Activate(BaseEditorToolItem item)
        {
            var activeTool = new EditorToolAreaSelectorActive();
            this.settings.ViewModelLocationSettings = activeTool.LocationSettingsViewModel;
            this.settings.ToolTerrain = this;
            return activeTool;
        }

        public void Apply(
            long seed,
            double noiseProbability,
            BoundsUshort selectionBounds,
            IProtoTile protoTileTarget,
            IProtoTile protoTileNoise)
        {
            Api.Assert(protoTileTarget != null,                "Please select target tile proto");
            Api.Assert(protoTileNoise != null,                 "Please select noise tile proto");
            Api.Assert(selectionBounds.Size.LengthSquared > 0, "Please select world area");
            Api.Assert(noiseProbability >= 0 || noiseProbability <= 1,
                       "Noise probability must be in range from 0 to 1 inclusive.");

            var random = new Random((int)seed);

            var world = Client.World;

            var tilesToModify = new List<Vector2Ushort>();

            for (var x = selectionBounds.MinX; x < selectionBounds.MaxX; x++)
            for (var y = selectionBounds.MinY; y < selectionBounds.MaxY; y++)
            {
                if (random.NextDouble() > noiseProbability)
                {
                    // do not process this tile
                    continue;
                }

                // check tile type
                var tilePosition = new Vector2Ushort(x, y);
                var tile = world.GetTile(tilePosition);
                if (tile.ProtoTile == protoTileTarget)
                {
                    tilesToModify.Add(tilePosition);
                }
            }

            if (tilesToModify.Count == 0)
            {
                return;
            }

            EditorClientSystem.DoAction(
                "Modify terrain tiles (noise)",
                onDo: () => tilesToModify.ChunkedInvoke(
                          5000,
                          chunk => this.CallServer(_ => _.ServerRemote_PlaceAt(chunk, protoTileNoise))),
                onUndo: () => tilesToModify.ChunkedInvoke(
                            5000,
                            chunk => this.CallServer(_ => _.ServerRemote_PlaceAt(chunk, protoTileTarget))));
        }

        public override FrameworkElement CreateSettingsControl()
        {
            var settingsControl = new EditorToolTerrainNoiseSettings();
            this.settings ??= new ViewModelEditorToolTerrainNoiseSettings();

            settingsControl.DataContext = this.settings;
            return settingsControl;
        }

        private void ServerRemote_PlaceAt(IList<Vector2Ushort> modifyRequests, IProtoTile protoTile)
        {
            if (modifyRequests.Count == 0)
            {
                throw new Exception("Incorrect modify request - at least one tile is required");
            }

            Logger.Important("Modify terrain at " + modifyRequests[0]);

            var worldService = Server.World;
            foreach (var tilePosition in modifyRequests)
            {
                var sourceTile = worldService.GetTile(tilePosition);
                worldService.SetTileData(
                    tilePosition,
                    protoTile,
                    tileHeight: sourceTile.Height,
                    isSlope: sourceTile.IsSlope,
                    isCliff: sourceTile.IsCliff);
            }

            worldService.FixMapTilesRecentlyModified();
        }
    }
}