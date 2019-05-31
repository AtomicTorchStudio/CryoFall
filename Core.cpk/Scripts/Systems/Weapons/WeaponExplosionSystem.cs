namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class WeaponExplosionSystem
    {
        /// <summary>
        /// Bomberman-style explosion penetrating the walls in a cross.
        /// </summary>
        public static void ServerProcessExplosionBomberman(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            int damageDistanceFullDamage,
            int damageDistanceMax,
            double damageDistanceDynamicObjectsOnly,
            WeaponFinalCache weaponFinalCache,
            Func<double, double> callbackCalculateDamageCoefByDistanceForStaticObjects,
            Func<double, double> callbackCalculateDamageCoefByDistanceForDynamicObjects)
        {
            var protoObjectExplosive = weaponFinalCache.ProtoObjectExplosive;
            Api.Assert(protoObjectExplosive != null,
                       "Weapon final cache should contain the exploded object");

            Api.Assert(damageDistanceMax >= damageDistanceFullDamage,
                       $"{nameof(damageDistanceMax)} must be >= {nameof(damageDistanceFullDamage)}");

            var world = Api.Server.World;
            var damagedObjects = new HashSet<IWorldObject>();

            ProcessExplosionDirection(-1, 0); // left
            ProcessExplosionDirection(0,  1); // top
            ProcessExplosionDirection(1,  0); // right
            ProcessExplosionDirection(0,  -1); // bottom

            ServerProcessExplosionCircle(positionEpicenter,
                                         physicsSpace,
                                         damageDistanceDynamicObjectsOnly,
                                         weaponFinalCache,
                                         damageOnlyDynamicObjects: true,
                                         callbackCalculateDamageCoefByDistance:
                                         callbackCalculateDamageCoefByDistanceForDynamicObjects);

            void ProcessExplosionDirection(int xOffset, int yOffset)
            {
                var fromPosition = positionEpicenter.ToVector2Ushort();
                for (var offsetIndex = 1; offsetIndex <= damageDistanceMax; offsetIndex++)
                {
                    var tile = world.GetTile(fromPosition.X + offsetIndex * xOffset,
                                             fromPosition.Y + offsetIndex * yOffset,
                                             logOutOfBounds: false);

                    if (!tile.IsValidTile
                        || tile.IsCliff)
                    {
                        return;
                    }

                    var tileStaticObjects = tile.StaticObjects;
                    IStaticWorldObject damagedObject = null;
                    foreach (var staticWorldObject in tileStaticObjects)
                    {
                        if (staticWorldObject.ProtoGameObject is IProtoObjectWall
                            || staticWorldObject.ProtoGameObject is IProtoObjectDoor)
                        {
                            // damage only walls and doors
                            damagedObject = staticWorldObject;
                            break;
                        }
                    }

                    if (damagedObject == null)
                    {
                        // no wall or door there
                        if (offsetIndex > damageDistanceFullDamage)
                        {
                            // stop damage propagation
                            return;
                        }

                        continue;
                    }

                    if (!damagedObjects.Add(damagedObject))
                    {
                        // the object is already damaged
                        // (from another direction which might be theoretically possible in some future cases)
                        continue;
                    }

                    var distanceToDamagedObject = offsetIndex;
                    var damageMultiplier =
                        callbackCalculateDamageCoefByDistanceForStaticObjects(distanceToDamagedObject);
                    damageMultiplier = MathHelper.Clamp(damageMultiplier, 0, 1);

                    var damageableProto = (IDamageableProtoWorldObject)damagedObject.ProtoGameObject;
                    damageableProto.SharedOnDamage(
                        weaponFinalCache,
                        damagedObject,
                        damageMultiplier,
                        out _,
                        out _);
                }
            }
        }

        public static void ServerProcessExplosionCircle(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            double damageDistanceMax,
            WeaponFinalCache weaponFinalCache,
            bool damageOnlyDynamicObjects,
            Func<double, double> callbackCalculateDamageCoefByDistance)
        {
            var protoObjectExplosive = weaponFinalCache.ProtoObjectExplosive;
            Api.Assert(protoObjectExplosive != null,
                       "Weapon final cache should contain the exploded object");

            var damageCandidates = new HashSet<IWorldObject>();

            // collect all damaged physics objects
            var collisionGroup = CollisionGroups.Default;
            using (var testResults = physicsSpace.TestCircle(positionEpicenter,
                                                             radius: damageDistanceMax,
                                                             collisionGroup: collisionGroup))
            {
                foreach (var testResult in testResults)
                {
                    var testResultPhysicsBody = testResult.PhysicsBody;
                    var damagedObject = testResultPhysicsBody.AssociatedWorldObject;

                    if (damageOnlyDynamicObjects
                        && damagedObject is IStaticWorldObject)
                    {
                        continue;
                    }

                    if (!(damagedObject?.ProtoWorldObject is IDamageableProtoWorldObject))
                    {
                        // non-damageable world object
                        continue;
                    }

                    damageCandidates.Add(damagedObject);
                }

                if (!damageOnlyDynamicObjects)
                {
                    // Collect all the damageable static objects in the explosion radius
                    // which don't have a collider colliding with the collision group.
                    var startTilePosition = positionEpicenter.ToVector2Ushort();
                    var damageDistanceMaxRounded = (int)damageDistanceMax;
                    var damageDistanceMaxSqr = damageDistanceMax * damageDistanceMax;
                    var minTileX = startTilePosition.X - damageDistanceMaxRounded;
                    var minTileY = startTilePosition.Y - damageDistanceMaxRounded;
                    var maxTileX = startTilePosition.X + damageDistanceMaxRounded;
                    var maxTileY = startTilePosition.Y + damageDistanceMaxRounded;

                    for (var x = minTileX; x <= maxTileX; x++)
                    for (var y = minTileY; y <= maxTileY; y++)
                    {
                        if (x < 0
                            || x > ushort.MaxValue
                            || y < 0
                            || y > ushort.MaxValue)
                        {
                            continue;
                        }

                        if (new Vector2Ushort((ushort)x, (ushort)y)
                                .TileSqrDistanceTo(startTilePosition)
                            > damageDistanceMaxSqr)
                        {
                            // too far
                            continue;
                        }

                        var tileObjects = Api.Server.World.GetStaticObjects(new Vector2Ushort((ushort)x, (ushort)y));
                        if (tileObjects.Count == 0)
                        {
                            continue;
                        }

                        foreach (var tileObject in tileObjects)
                        {
                            if (!(tileObject.ProtoStaticWorldObject is IDamageableProtoWorldObject))
                            {
                                // non-damageable
                                continue;
                            }

                            if (tileObject.PhysicsBody.HasAnyShapeCollidingWithGroup(collisionGroup))
                            {
                                // has a collider colliding with the collision group so we ignore this
                                continue;
                            }

                            damageCandidates.Add(tileObject);
                        }
                    }
                }

                // order by distance to explosion center
                var orderedDamagedObjects =
                    damageCandidates.OrderBy(ServerExplosionGetDistanceToEpicenter(positionEpicenter));
                // process all damaged objects
                foreach (var damagedObject in orderedDamagedObjects)
                {
                    if (ServerHasObstacleForExplosion(physicsSpace, positionEpicenter, damagedObject))
                    {
                        continue;
                    }

                    var distanceToDamagedObject = ServerCalculateDistanceToDamagedObject(positionEpicenter,
                                                                                         damagedObject);
                    var damageMultiplier = callbackCalculateDamageCoefByDistance(distanceToDamagedObject);
                    damageMultiplier = MathHelper.Clamp(damageMultiplier, 0, 1);

                    var damageableProto = (IDamageableProtoWorldObject)damagedObject.ProtoGameObject;
                    damageableProto.SharedOnDamage(
                        weaponFinalCache,
                        damagedObject,
                        damageMultiplier,
                        out _,
                        out _);
                }
            }
        }

        private static double ServerCalculateDistanceToDamagedObject(Vector2D fromPosition, IWorldObject worldObject)
        {
            switch (worldObject)
            {
                case IDynamicWorldObject dynamicWorldObject:
                    return fromPosition.DistanceTo(dynamicWorldObject.Position);

                case IStaticWorldObject staticWorldObject:
                    var minDistance = double.MaxValue;
                    var fromTilePosition = fromPosition.ToVector2Ushort();

                    foreach (var occupiedTile in staticWorldObject.OccupiedTiles)
                    {
                        var distance = fromTilePosition.TileDistanceTo(occupiedTile.Position);
                        if (minDistance > distance)
                        {
                            minDistance = distance;
                        }
                    }

                    return minDistance;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Func<IWorldObject, Vector2D> ServerExplosionGetDistanceToEpicenter(Vector2D positionEpicenter)
        {
            return testResult => ServerGetClosestPointToExplosionEpicenter(testResult.PhysicsBody, positionEpicenter)
                                 - positionEpicenter;
        }

        private static Vector2D ServerGetClosestPointToExplosionEpicenter(
            IPhysicsBody physicsBody,
            Vector2D positionEpicenter)
        {
            if (!(physicsBody.AssociatedWorldObject?.ProtoWorldObject
                      is IProtoStaticWorldObject protoStaticWorldObject))
            {
                return physicsBody.ClampPointInside(
                    positionEpicenter,
                    CollisionGroups.Default,
                    out _);
            }

            // find closest tile in layout and return its center
            var closestDistanceSqr = double.MaxValue;
            var tilePosition = physicsBody.AssociatedWorldObject.TilePosition;
            var closestPosition = tilePosition.ToVector2D();

            foreach (var tileOffset in protoStaticWorldObject.Layout.TileOffsets)
            {
                Vector2D pos = (tilePosition.X + tileOffset.X,
                                tilePosition.Y + tileOffset.Y);
                var distanceSqr = pos.DistanceSquaredTo(positionEpicenter);
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestPosition = pos;
                }
            }

            return (closestPosition.X + 0.5,
                    closestPosition.Y + 0.5);
        }

        private static bool ServerHasObstacleForExplosion(
            IPhysicsSpace physicsSpace,
            Vector2D positionEpicenter,
            IWorldObject targetWorldObject)
        {
            //// doesn't work as expected due to the penetration resolution not implemented yet
            //// for the line segment collision with other shapes
            //// get closest point to the explosion epicenter
            ////var targetPosition = targetWorldObject.PhysicsBody.ClampPointInside(
            ////    positionEpicenter,
            ////    CollisionGroups.Default);
            //
            //var targetPosition = targetWorldObject.PhysicsBody.Position
            //                     + targetWorldObject.PhysicsBody.CenterOffset;

            if (targetWorldObject.PhysicsBody == null)
            {
                return false;
            }

            var targetPosition = ServerGetClosestPointToExplosionEpicenter(targetWorldObject.PhysicsBody,
                                                                           positionEpicenter);

            using (var obstaclesOnTheWay = physicsSpace.TestLine(
                positionEpicenter,
                targetPosition,
                collisionGroup: CollisionGroups.Default))
            {
                //obstaclesOnTheWay.SortBy(
                //    ServerExplosionGetDistanceToEpicenter(positionEpicenter));

                foreach (var testResult in obstaclesOnTheWay)
                {
                    var testPhysicsBody = testResult.PhysicsBody;
                    if (testPhysicsBody.AssociatedProtoTile != null)
                    {
                        // obstacle tile on the way
                        return true;
                    }

                    var testWorldObject = testPhysicsBody.AssociatedWorldObject;
                    if (testWorldObject == targetWorldObject)
                    {
                        // not an obstacle - it's the target object itself
                        // stop checking collisions as we've reached the target object
                        return false;
                    }

                    if (testWorldObject is ICharacter)
                    {
                        // not an obstacle - character is not considered as an obstacle for the explosion
                        continue;
                    }

                    if (testWorldObject.ProtoWorldObject is IDamageableProtoWorldObject damageableProtoWorldObject
                        && damageableProtoWorldObject.ObstacleBlockDamageCoef < 1)
                    {
                        // damage goes through
                        continue;
                    }

                    // obstacle object on the way
                    return true;
                }

                return false;
            }
        }
    }
}