namespace AtomicTorch.CBND.CoreMod.Editor.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    [RemoteAuthorizeServerOperator]
    public class TerrainEditingSystem : ProtoSystem<TerrainEditingSystem>
    {
        public override string Name => nameof(TerrainEditingSystem);

        public static void ClientModifyTerrain(List<TileModifyRequest> tilesToModify)
        {
            if (tilesToModify.Count == 0)
            {
                return;
            }

            // separate on groups of world chunk size
            var worldChunkSize = 10 * ScriptingConstants.WorldChunkSize;

            var modificationGroups = tilesToModify
                                     .GroupBy(g => new Vector2Ushort((ushort)(g.TilePosition.X / worldChunkSize),
                                                                     (ushort)(g.TilePosition.Y / worldChunkSize)))
                                     .Select(g => g.ToList())
                                     .ToList();

            var worldService = Client.World;
            var revertGroups = modificationGroups
                               .Select(g => g.Select(
                                                 t => new TileModifyRequest(worldService.GetTile(t.TilePosition)))
                                             .ToList())
                               .ToList();

            EditorClientActionsHistorySystem.DoAction(
                "Modify terrain tiles",
                onDo: () => modificationGroups.ForEach(
                          chunk => Instance.CallServer(_ => _.ServerRemote_PlaceAt(chunk))),
                onUndo: () => revertGroups.ForEach(
                            chunk => Instance.CallServer(_ => _.ServerRemote_PlaceAt(chunk))),
                canGroupWithPreviousAction: true);
        }

        [RemoteCallSettings(DeliveryMode.Default,
                            timeInterval: 0,
                            clientMaxSendQueueSize: byte.MaxValue)]
        private void ServerRemote_PlaceAt(List<TileModifyRequest> modifyRequests)
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
                    tileHeight: request.Height,
                    isSlope: request.IsSlope,
                    isCliff: false);
            }

            worldService.FixMapTilesRecentlyModified();
        }

        public readonly struct TileModifyRequest : IRemoteCallParameter
        {
            public readonly byte Height;

            public readonly bool IsSlope;

            public readonly byte ProtoTileSessionIndex;

            public readonly Vector2Ushort TilePosition;

            public TileModifyRequest(
                Vector2Ushort tilePosition,
                byte protoTileSessionIndex,
                byte height,
                bool isSlope)
            {
                this.TilePosition = tilePosition;
                this.Height = height;
                this.IsSlope = isSlope;
                this.ProtoTileSessionIndex = protoTileSessionIndex;
            }

            public TileModifyRequest(Tile tile)
                : this(tile.Position,
                       tile.ProtoTile.SessionIndex,
                       tile.Height,
                       tile.IsSlope)
            {
            }

            public TileModifyRequest ApplyOffset(Vector2Ushort offset)
            {
                return new(this.TilePosition.AddAndClamp(offset),
                           this.ProtoTileSessionIndex,
                           this.Height,
                           this.IsSlope);
            }
        }
    }
}