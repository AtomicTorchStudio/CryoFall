namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectPropPlatform : ProtoObjectProp, IProtoObjectMovementSurface
    {
        /// <summary>
        /// Same speed boost as for the road tiles by default.
        /// </summary>
        public virtual double CharacterMoveSpeedMultiplier => 1.15;

        /// <summary>
        /// Gets sound material of ground (used for movement footsteps and similar sounds).
        /// </summary>
        public virtual GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Stone;

        public override StaticObjectKind Kind => StaticObjectKind.Platform;

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            if (Api.IsEditor)
            {
                ServerEditorReplaceBridgeTilesWithPlaceholder(gameObject);
            }
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            using var occupiedTilePositions = Api.Shared.WrapInTempList(
                data.GameObject.OccupiedTilePositions);

            Client.World.RebuildTilePhysics(occupiedTilePositions.AsList());
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrder = DrawOrder.Floor - 1;
        }

        protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
        {
            tileRequirements.Clear()
                            .Add("No other platforms allowed",
                                 c => c.Tile.StaticObjects.All(
                                     o => o.ProtoStaticWorldObject.Kind != StaticObjectKind.Platform));
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            using var occupiedTilePositions = Api.Shared.WrapInTempList(
                data.GameObject.OccupiedTilePositions);

            if (Api.IsEditor)
            {
                ServerEditorReplaceWaterTilesWithBridge(data.GameObject);
            }

            Server.World.RebuildTilePhysics(occupiedTilePositions.AsList());
        }

        private static void ServerEditorReplaceBridgeTilesWithPlaceholder(IStaticWorldObject gameObject)
        {
            var world = Server.World;

            using var occupiedTilePositions = Api.Shared.GetTempList<Vector2Ushort>();
            foreach (var tile in gameObject.OccupiedTiles)
            {
                var tilePosition = tile.Position;
                occupiedTilePositions.Add(tilePosition);
                if (tile.ProtoTile is not IProtoTileWater)
                {
                    continue;
                }

                // replace proto tile with a sea water tile
                world.SetTileData(tilePosition,
                                  Api.GetProtoEntity<TileWaterSea>(),
                                  tile.Height,
                                  tile.IsSlope,
                                  tile.IsCliff);
            }

            // even if nothing has changed, due to the removal of the platform,
            // the tile cliffs may have changed - mark all previously occupied tiles as modified
            world.MarkTilesAsModified(occupiedTilePositions.AsList());

            world.FixMapTilesRecentlyModified();
        }

        // In Editor when the platform prop is initialized
        // the game server will try to replace the water tiles
        // under the platform with a bridge tile variant.
        private static void ServerEditorReplaceWaterTilesWithBridge(IStaticWorldObject gameObject)
        {
            IWorldServerService world = null;
            foreach (var tile in gameObject.OccupiedTiles)
            {
                var currentProtoTile = tile.ProtoTile;
                if (currentProtoTile is not IProtoTileWater protoTileWater)
                {
                    if (tile.IsCliffOrSlope)
                    {
                        (world ??= Server.World)
                            .SetTileData(tile.Position,
                                         currentProtoTile,
                                         tile.Height,
                                         isSlope: false,
                                         isCliff: false);
                    }

                    continue;
                }

                var bridgeProtoTile = protoTileWater.BridgeProtoTile;
                if (ReferenceEquals(protoTileWater, bridgeProtoTile)
                    || bridgeProtoTile is null)
                {
                    continue;
                }

                // replace proto tile with bridge
                (world ??= Server.World)
                    .SetTileData(tile.Position,
                                 bridgeProtoTile,
                                 tile.Height,
                                 isSlope: false,
                                 isCliff: false);
            }

            world?.FixMapTilesRecentlyModified();
        }
    }
}