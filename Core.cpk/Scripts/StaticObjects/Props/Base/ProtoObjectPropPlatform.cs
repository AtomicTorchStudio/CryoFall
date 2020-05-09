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
            IWorldServerService serverWorld = null;

            foreach (var tile in gameObject.OccupiedTiles)
            {
                if (!(tile.ProtoTile is IProtoTileWater))
                {
                    continue;
                }

                serverWorld = Server.World;

                // replace proto tile with a sea water tile
                serverWorld.SetTileData(tile.Position,
                                        Api.GetProtoEntity<TileWaterSea>(),
                                        tile.Height,
                                        tile.IsSlope,
                                        tile.IsCliff);
            }

            serverWorld?.FixMapTilesRecentlyModified();
        }

        // In Editor when the platform prop is initialized
        // the game server will try to replace the water tiles
        // under the platform with a bridge tile variant.
        private static void ServerEditorReplaceWaterTilesWithBridge(IStaticWorldObject gameObject)
        {
            IWorldServerService serverWorld = null;

            foreach (var tile in gameObject.OccupiedTiles)
            {
                if (!(tile.ProtoTile is IProtoTileWater protoTileWater))
                {
                    continue;
                }

                var bridgeProtoTile = protoTileWater.BridgeProtoTile;
                if (ReferenceEquals(protoTileWater, bridgeProtoTile)
                    || bridgeProtoTile is null)
                {
                    continue;
                }

                serverWorld = Server.World;

                // replace proto tile with bridge
                serverWorld.SetTileData(tile.Position,
                                        bridgeProtoTile,
                                        tile.Height,
                                        tile.IsSlope,
                                        tile.IsCliff);
            }

            serverWorld?.FixMapTilesRecentlyModified();
        }
    }
}