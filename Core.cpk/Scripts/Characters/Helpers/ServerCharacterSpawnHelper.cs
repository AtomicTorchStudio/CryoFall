namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ServerCharacterSpawnHelper
    {
        // no players distance for spawning a creature
        public const double SpawnMobNoPlayersRadius = 8;

        public const double SpawnNoPhysicsObjectsRadius = 1;

        public const double SpawnNoPlayerBuiltStructuresRadius = 8;

        // no mobs distance for spawning a player character
        public const double SpawnPlayerNoMobsRadius = 8;

        private static readonly IWorldServerService ServerWorldService = Api.Server.World;

        public static bool IsPositionValidForCharacterSpawn(
            Vector2D position,
            bool isPlayer)
        {
            if (!IsValidSpawnPosition(position))
            {
                return false;
            }

            var physicsSpace = ServerWorldService.GetPhysicsSpace();
            foreach (var t in physicsSpace.TestCircle(position,
                                                      SpawnNoPlayerBuiltStructuresRadius,
                                                      CollisionGroups.Default,
                                                      sendDebugEvent: false).EnumerateAndDispose())
            {
                if (t.PhysicsBody.AssociatedWorldObject?.ProtoWorldObject is IProtoObjectStructure)
                {
                    // some structure nearby
                    return false;
                }
            }

            if (isPlayer)
            {
                // check if mobs nearby
                foreach (var t in physicsSpace.TestCircle(position,
                                                          SpawnPlayerNoMobsRadius,
                                                          CollisionGroups.Default,
                                                          sendDebugEvent: false).EnumerateAndDispose())
                {
                    if (t.PhysicsBody.AssociatedWorldObject is ICharacter otherCharacter
                        && otherCharacter.IsNpc)
                    {
                        // mobs nearby
                        return false;
                    }
                }
            }
            else // a mob
            {
                // check if players nearby
                foreach (var t in physicsSpace.TestCircle(position,
                                                          SpawnMobNoPlayersRadius,
                                                          CollisionGroups.Default,
                                                          sendDebugEvent: false).EnumerateAndDispose())
                {
                    if (t.PhysicsBody.AssociatedWorldObject is ICharacter otherCharacter
                        && !otherCharacter.IsNpc)
                    {
                        // players nearby
                        return false;
                    }
                }
            }

            // check if any physics object nearby
            using (var objectsNearby = physicsSpace.TestCircle(position,
                                                               SpawnNoPhysicsObjectsRadius,
                                                               CollisionGroups.Default,
                                                               sendDebugEvent: false))
            {
                if (objectsNearby.Count > 0)
                {
                    // some physical object nearby
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidSpawnPosition(Vector2D position)
        {
            var tile = ServerWorldService.GetTile(position.ToVector2Ushort());
            return IsValidSpawnTileInternal(tile);
        }

        public static bool IsValidSpawnTile(Tile tile, bool checkNeighborTiles)
        {
            if (!IsValidSpawnTileInternal(tile))
            {
                return false;
            }

            if (!checkNeighborTiles)
            {
                return true;
            }

            foreach (var t in tile.EightNeighborTiles)
            {
                if (!IsValidSpawnTileInternal(t))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsValidSpawnTileInternal(Tile tile)
        {
            return tile.IsValidTile
                   && !tile.IsCliffOrSlope
                   && tile.ProtoTile.Kind == TileKind.Solid;
        }
    }
}