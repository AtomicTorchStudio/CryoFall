namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.GameApi.Data.World;

    public static class DoorHelper
    {
        public static bool IsHorizontalDoorNeeded(Tile tile, bool checkExistingDoor)
        {
            if (checkExistingDoor)
            {
                var door = tile.StaticObjects
                               .FirstOrDefault(o => o.ProtoStaticWorldObject is IProtoObjectDoor);
                if (door != null)
                {
                    // the tile already has a door - return the door orientation
                    return door.GetPublicState<ObjectDoorPublicState>()
                               .IsHorizontalDoor;
                }
            }

            var tileLeft = tile.NeighborTileLeft;
            var tileRight = tile.NeighborTileRight;
            var tileUp = tile.NeighborTileUp;
            var tileDown = tile.NeighborTileDown;

            // check neighbor doors
            if (TileHasDoor(tileUp))
            {
                return IsHorizontalDoor(tileUp);
            }

            if (TileHasDoor(tileDown))
            {
                return IsHorizontalDoor(tileDown);
            }

            if (TileHasDoor(tileLeft))
            {
                return IsHorizontalDoor(tileLeft);
            }

            if (TileHasDoor(tileRight))
            {
                return IsHorizontalDoor(tileRight);
            }

            // check neighbor walls (and blueprints)
            var hasLeftWall = TileHasObjectsOfType<IProtoObjectWall>(tileLeft);
            var hasRightWall = TileHasObjectsOfType<IProtoObjectWall>(tileRight);
            if (hasLeftWall && hasRightWall)
            {
                // definitely horizontal door
                return true;
            }

            var hasUpWall = TileHasObjectsOfType<IProtoObjectWall>(tileUp);
            var hasDownWall = TileHasObjectsOfType<IProtoObjectWall>(tileDown);
            if (hasUpWall && hasDownWall)
            {
                // definitely vertical door
                return false;
            }

            if (hasLeftWall || hasRightWall)
            {
                // prefer horizontal door
                return true;
            }

            if (hasUpWall || hasDownWall)
            {
                // prefer vertical door
                return false;
            }

            // check any neighbor static objects
            var hasUpObject = TileHasStructures(tileUp);
            var hasDownObject = TileHasStructures(tileDown);
            if (hasUpObject && hasDownObject)
            {
                // definitely vertical door
                return false;
            }

            var hasLeftObject = TileHasStructures(tileLeft);
            var hasRightObject = TileHasStructures(tileRight);
            if (hasLeftObject && hasRightObject)
            {
                // definitely horizontal door
                return true;
            }

            if (hasLeftObject || hasRightObject)
            {
                // prefer horizontal door
                return true;
            }

            if (hasUpObject || hasDownObject)
            {
                // prefer vertical door
                return false;
            }

            // default - horizontal door
            return true;
        }

        public static void RefreshDoorType(IStaticWorldObject door)
        {
            if (!(door.ProtoGameObject is IProtoObjectDoor))
            {
                // not a door
                return;
            }

            door.GetPublicState<ObjectDoorPublicState>().IsHorizontalDoor
                = IsHorizontalDoorNeeded(door.OccupiedTile, checkExistingDoor: false);
        }

        public static void RefreshNeighborDoorType(in Tile tile)
        {
            // this mechanic is causing the rotation of the already placed doors which is not expected by players
            return;

            foreach (var neighborTile in tile.FourNeighborTiles)
            {
                foreach (var staticWorldObject in neighborTile.StaticObjects)
                {
                    if (staticWorldObject.ProtoGameObject is IProtoObjectDoor)
                    {
                        RefreshDoorType(staticWorldObject);
                    }
                }
            }
        }

        private static bool IsHorizontalDoor(in Tile tileUp)
        {
            var door = tileUp.StaticObjects
                             .First(o => o.ProtoStaticWorldObject is IProtoObjectDoor);
            return door.GetPublicState<ObjectDoorPublicState>()
                       .IsHorizontalDoor;
        }

        private static bool TileHasDoor(in Tile tile)
        {
            foreach (var worldObject in tile.StaticObjects)
            {
                if (worldObject.ProtoStaticWorldObject is IProtoObjectDoor)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TileHasObjectsOfType<TProtoStaticWorldObject>(in Tile tile)
            where TProtoStaticWorldObject : IProtoStaticWorldObject
        {
            foreach (var worldObject in tile.StaticObjects)
            {
                if (worldObject.ProtoStaticWorldObject is TProtoStaticWorldObject
                    || (worldObject.ProtoStaticWorldObject is ProtoObjectConstructionSite
                        && ProtoObjectConstructionSite.SharedIsConstructionOf(worldObject,
                                                                              typeof(TProtoStaticWorldObject))))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TileHasStructures(in Tile tile)
        {
            foreach (var worldObject in tile.StaticObjects)
            {
                if (worldObject.ProtoStaticWorldObject is IProtoObjectStructure protoObjectStructure
                    && protoObjectStructure.Kind == StaticObjectKind.Structure)
                {
                    return true;
                }
            }

            return false;
        }
    }
}