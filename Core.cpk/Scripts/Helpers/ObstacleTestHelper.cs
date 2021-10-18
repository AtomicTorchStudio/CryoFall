namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public static class ObstacleTestHelper
    {
        public static bool SharedHasObstaclesInTheWay(
            Vector2D fromPosition,
            IPhysicsSpace physicsSpace,
            [CanBeNull] IWorldObject worldObject,
            Vector2D worldObjectCenter,
            Vector2D worldObjectPointClosestToCharacter,
            bool sendDebugEvents,
            CollisionGroup collisionGroup)
        {
            // let's test by casting rays from "fromPosition" (usually it's the character's center) to:
            // 0) world object center
            // 1) world object point closest to the character
            // 2) combined - take X from center, take Y from closest
            // 3) combined - take X from closest, take Y from center
            if (TestHasObstacle(worldObjectCenter)
                && TestHasObstacle(worldObjectPointClosestToCharacter)
                && TestHasObstacle((worldObjectCenter.X,
                                    worldObjectPointClosestToCharacter.Y))
                && TestHasObstacle((worldObjectPointClosestToCharacter.X, worldObjectCenter.Y)))
            {
                // has obstacle
                return true;
            }

            return false;

            // local method for testing if there is an obstacle from current to the specified position
            bool TestHasObstacle(Vector2D toPosition)
            {
                using var obstaclesInTheWay = physicsSpace.TestLine(
                    fromPosition,
                    toPosition,
                    collisionGroup,
                    sendDebugEvent: sendDebugEvents);
                foreach (var test in obstaclesInTheWay.AsList())
                {
                    var testPhysicsBody = test.PhysicsBody;
                    if (testPhysicsBody.AssociatedProtoTile is not null)
                    {
                        // obstacle tile on the way
                        return true;
                    }

                    var testWorldObject = testPhysicsBody.AssociatedWorldObject;
                    if (testWorldObject is null)
                    {
                        // unknown obstacle (such as boss event barrier that is used during the boss spawn delay in PvE)
                        return true;
                    }

                    if (ReferenceEquals(testWorldObject, worldObject))
                    {
                        // not an obstacle - it's the target object itself
                        // stop checking collisions as we've reached the target object
                        return false;
                    }

                    if (testWorldObject is IDynamicWorldObject)
                    {
                        // dynamic world objects are not assumed as an obstacle
                        continue;
                    }

                    if (!testWorldObject.ProtoWorldObject
                                        .SharedIsAllowedObjectToInteractThrough(testWorldObject))
                    {
                        // obstacle object on the way
                        return true;
                    }
                }

                // no obstacles
                return false;
            }
        }

        public static bool SharedHasObstaclesInTheWay(
            Vector2D fromPosition,
            IPhysicsSpace physicsSpace,
            IWorldObject worldObject,
            bool sendDebugEvents)
        {
            var worldObjectCenter = SharedGetWorldObjectCenter(worldObject);
            var worldObjectPointClosestToCharacter = worldObject.PhysicsBody.ClampPointInside(
                fromPosition,
                CollisionGroups.Default,
                out var isSuccess);

            if (!isSuccess)
            {
                // the physics body seems to not have the default collider, let's check for the click area instead
                worldObjectPointClosestToCharacter = worldObject.PhysicsBody.ClampPointInside(
                    fromPosition,
                    CollisionGroups.ClickArea,
                    out _);
            }

            return SharedHasObstaclesInTheWay(fromPosition,
                                              physicsSpace,
                                              worldObject,
                                              worldObjectCenter,
                                              worldObjectPointClosestToCharacter,
                                              sendDebugEvents,
                                              CollisionGroups.Default);

            static Vector2D SharedGetWorldObjectCenter(IWorldObject worldObject)
            {
                if (worldObject is IDynamicWorldObject dynamicWorldObject)
                {
                    return dynamicWorldObject.Position;
                }

                return worldObject.TilePosition.ToVector2D()
                       + worldObject.PhysicsBody.CenterOffset;
            }
        }

        public static bool SharedHasObstaclesInTheWay(
            Vector2D fromPosition,
            IPhysicsSpace physicsSpace,
            Vector2D worldObjectCenter,
            Vector2D worldObjectPointClosestToCharacter,
            bool sendDebugEvents)
        {
            return SharedHasObstaclesInTheWay(fromPosition,
                                              physicsSpace,
                                              worldObject: null,
                                              worldObjectCenter: worldObjectCenter,
                                              worldObjectPointClosestToCharacter: worldObjectPointClosestToCharacter,
                                              sendDebugEvents: sendDebugEvents,
                                              collisionGroup: CollisionGroups.Default);
        }
    }
}