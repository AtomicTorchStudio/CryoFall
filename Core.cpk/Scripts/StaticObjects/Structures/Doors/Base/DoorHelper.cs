namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors
{
    using System.Linq;
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
            var hasLeftDoor = IsDoorJoinableWith<IProtoObjectDoor>(tileLeft);
            var hasRightDoor = IsDoorJoinableWith<IProtoObjectDoor>(tileRight);
            var hasUpDoor = IsDoorJoinableWith<IProtoObjectDoor>(tileUp);
            var hasDownDoor = IsDoorJoinableWith<IProtoObjectDoor>(tileDown);

            if (hasUpDoor)
            {
                return IsHorizontalDoor(tileUp);
            }

            if (hasDownDoor)
            {
                return IsHorizontalDoor(tileDown);
            }

            if (hasLeftDoor)
            {
                return IsHorizontalDoor(tileLeft);
            }

            if (hasRightDoor)
            {
                return IsHorizontalDoor(tileRight);
            }

            // check neighbor walls
            var hasLeftWall = IsDoorJoinableWith<IProtoObjectWall>(tileLeft);
            var hasRightWall = IsDoorJoinableWith<IProtoObjectWall>(tileRight);
            if (hasLeftWall && hasRightWall)
            {
                // definitely horizontal door
                return true;
            }

            var hasUpWall = IsDoorJoinableWith<IProtoObjectWall>(tileUp);
            var hasDownWall = IsDoorJoinableWith<IProtoObjectWall>(tileDown);
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
            var hasLeftObject = IsDoorJoinableWith<IProtoStaticWorldObject>(tileLeft);
            var hasRightObject = IsDoorJoinableWith<IProtoStaticWorldObject>(tileRight);
            var hasUpObject = IsDoorJoinableWith<IProtoStaticWorldObject>(tileUp);
            var hasDownObject = IsDoorJoinableWith<IProtoStaticWorldObject>(tileDown);

            if (hasUpObject && hasDownObject)
            {
                // definitely vertical door
                return false;
            }

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

        public static void RefreshNeighborDoorType(Tile tile)
        {
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

        private static bool IsDoorJoinableWith<TProtoStaticWorldObject>(Tile tile)
            where TProtoStaticWorldObject : IProtoStaticWorldObject
        {
            foreach (var worldObject in tile.StaticObjects)
            {
                if (worldObject.ProtoStaticWorldObject is TProtoStaticWorldObject)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsHorizontalDoor(Tile tileUp)
        {
            var door = tileUp.StaticObjects
                             .First(o => o.ProtoStaticWorldObject is IProtoObjectDoor);
            return door.GetPublicState<ObjectDoorPublicState>().IsHorizontalDoor;
        }
    }
}