namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;
    using static SoundPresets.ObjectMaterial;

    public class WeaponExplosionSystem : ProtoSystem<WeaponExplosionSystem>
    {
        public static readonly IReadOnlyWeaponHitSparksPreset ExplosionHitSparksPreset
            = WeaponHitSparksPresets.Firearm;

        public static readonly ReadOnlySoundPreset<ObjectMaterial> SoundPresetHitExplosion
            = new SoundPreset<ObjectMaterial>(MaterialHitsSoundPresets.HitSoundDistancePreset)
              .Add(SoftTissues, "Hit/Ranged/SoftTissues")
              .Add(HardTissues, "Hit/Ranged/HardTissues")
              .Add(SolidGround, "Hit/Ranged/SolidGround")
              .Add(Vegetation,  "Hit/Ranged/Vegetation")
              .Add(Wood,        "Hit/Ranged/Wood")
              .Add(Stone,       "Hit/Ranged/Stone")
              .Add(Metal,       "Hit/Ranged/Metal")
              .Add(Glass,       "Hit/Ranged/Glass");

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
            Api.Assert(damageDistanceMax >= damageDistanceFullDamage,
                       $"{nameof(damageDistanceMax)} must be >= {nameof(damageDistanceFullDamage)}");

            var playerCharacterSkills = weaponFinalCache.Character?.SharedGetSkills();
            var protoWeaponSkill = playerCharacterSkills is not null
                                       ? weaponFinalCache.ProtoWeapon?.WeaponSkillProto
                                       : null;

            var world = Api.Server.World;
            var allDamagedObjects = new HashSet<IWorldObject>();

            ProcessExplosionDirection(-1, 0);  // left
            ProcessExplosionDirection(0,  1);  // top
            ProcessExplosionDirection(1,  0);  // right
            ProcessExplosionDirection(0,  -1); // bottom

            ServerProcessExplosionCircle(positionEpicenter,
                                         physicsSpace,
                                         damageDistanceDynamicObjectsOnly,
                                         weaponFinalCache,
                                         damageOnlyDynamicObjects: true,
                                         isDamageThroughObstacles: false,
                                         callbackCalculateDamageCoefByDistanceForStaticObjects:
                                         callbackCalculateDamageCoefByDistanceForStaticObjects,
                                         callbackCalculateDamageCoefByDistanceForDynamicObjects:
                                         callbackCalculateDamageCoefByDistanceForDynamicObjects);

            void ProcessExplosionDirection(int xOffset, int yOffset)
            {
                foreach (var (damagedObject, offsetIndex) in
                         SharedEnumerateExplosionBombermanDirectionTilesWithTargets(positionEpicenter,
                             damageDistanceFullDamage,
                             damageDistanceMax,
                             world,
                             xOffset,
                             yOffset))
                {
                    if (damagedObject is null)
                    {
                        continue;
                    }

                    if (!allDamagedObjects.Add(damagedObject))
                    {
                        // the object is already damaged
                        // (from another direction which might be theoretically possible in some future cases)
                        continue;
                    }

                    var distanceToDamagedObject = offsetIndex;
                    // this explosion pattern selects only the static objects as targets
                    var damagePreMultiplier = callbackCalculateDamageCoefByDistanceForStaticObjects(
                        distanceToDamagedObject);
                    damagePreMultiplier = MathHelper.Clamp(damagePreMultiplier, 0, 1);

                    var damageableProto = (IDamageableProtoWorldObject)damagedObject.ProtoGameObject;
                    damageableProto.SharedOnDamage(
                        weaponFinalCache,
                        damagedObject,
                        damagePreMultiplier,
                        damagePostMultiplier: 1.0,
                        out _,
                        out var damageApplied);

                    if (Api.IsServer)
                    {
                        if (damageApplied > 0)
                        {
                            // give experience for damage
                            protoWeaponSkill?.ServerOnDamageApplied(playerCharacterSkills,
                                                                    damagedObject,
                                                                    damageApplied);
                        }

                        weaponFinalCache.ProtoExplosive?.ServerOnObjectHitByExplosion(damagedObject,
                            damageApplied,
                            weaponFinalCache);
                    }
                }
            }
        }

        public static void ServerProcessExplosionCircle(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            double damageDistanceMax,
            WeaponFinalCache weaponFinalCache,
            bool damageOnlyDynamicObjects,
            bool isDamageThroughObstacles,
            Func<double, double> callbackCalculateDamageCoefByDistanceForStaticObjects,
            Func<double, double> callbackCalculateDamageCoefByDistanceForDynamicObjects,
            [CanBeNull] CollisionGroup[] collisionGroups = null,
            Func<IWorldObject, bool> filterCanDamage = null)
        {
            var playerCharacterSkills = weaponFinalCache.Character?.SharedGetSkills();
            var protoWeaponSkill = playerCharacterSkills is not null
                                       ? weaponFinalCache.ProtoWeapon?.WeaponSkillProto
                                       : null;

            // collect all damaged objects via physics space
            var damageCandidates = new HashSet<IWorldObject>();

            collisionGroups ??= new[]
            {
                CollisionGroup.Default,
                CollisionGroups.HitboxMelee,
                CollisionGroups.HitboxRanged
            };

            var defaultCollisionGroup = collisionGroups[0];

            foreach (var collisionGroup in collisionGroups)
            {
                CollectDamagedPhysicalObjects(collisionGroup);
            }

            void CollectDamagedPhysicalObjects(CollisionGroup collisionGroup)
            {
                using var testResults = physicsSpace.TestCircle(positionEpicenter,
                                                                radius: damageDistanceMax,
                                                                collisionGroup: collisionGroup);
                foreach (var testResult in testResults.AsList())
                {
                    var testResultPhysicsBody = testResult.PhysicsBody;
                    var damagedObject = testResultPhysicsBody.AssociatedWorldObject;
                    if (damagedObject is null)
                    {
                        continue;
                    }

                    if (damageOnlyDynamicObjects
                        && damagedObject is IStaticWorldObject)
                    {
                        continue;
                    }

                    if (damagedObject?.ProtoWorldObject is not IDamageableProtoWorldObject)
                    {
                        // non-damageable world object
                        continue;
                    }

                    if (filterCanDamage is not null
                        && !filterCanDamage.Invoke(damagedObject))
                    {
                        continue;
                    }

                    damageCandidates.Add(damagedObject);
                }
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
                        if (tileObject.ProtoStaticWorldObject is not IDamageableProtoWorldObject)
                        {
                            // non-damageable
                            continue;
                        }

                        if (tileObject.PhysicsBody.HasAnyShapeCollidingWithGroup(defaultCollisionGroup))
                        {
                            // has a collider colliding with the collision group so we ignore this
                            continue;
                        }

                        if (filterCanDamage is not null
                            && !filterCanDamage.Invoke(tileObject))
                        {
                            continue;
                        }

                        damageCandidates.Add(tileObject);
                    }
                }
            }

            // order by distance to explosion center
            var orderedDamageCandidates = damageCandidates.OrderBy(
                ServerExplosionGetDistanceToEpicenter(positionEpicenter, defaultCollisionGroup));

            var hitCharacters = new List<WeaponHitData>();

            // process all damage candidates
            foreach (var damagedObject in orderedDamageCandidates)
            {
                if (!isDamageThroughObstacles
                    && ServerHasObstacleForExplosion(physicsSpace,
                                                     positionEpicenter,
                                                     damagedObject,
                                                     defaultCollisionGroup))
                {
                    continue;
                }

                var distanceToDamagedObject = ServerCalculateDistanceToDamagedObject(positionEpicenter,
                    damagedObject);
                var damagePreMultiplier =
                    damagedObject is IDynamicWorldObject
                        ? callbackCalculateDamageCoefByDistanceForDynamicObjects(distanceToDamagedObject)
                        : callbackCalculateDamageCoefByDistanceForStaticObjects(distanceToDamagedObject);

                damagePreMultiplier = MathHelper.Clamp(damagePreMultiplier, 0, 1);

                var damageableProto = (IDamageableProtoWorldObject)damagedObject.ProtoGameObject;
                damageableProto.SharedOnDamage(
                    weaponFinalCache,
                    damagedObject,
                    damagePreMultiplier,
                    damagePostMultiplier: 1.0,
                    out _,
                    out var damageApplied);

                if (damageApplied > 0
                    && damagedObject is ICharacter damagedCharacter)
                {
                    hitCharacters.Add(new WeaponHitData(damagedCharacter,
                                                        (0,
                                                         damagedCharacter
                                                             .ProtoCharacter.CharacterWorldWeaponOffsetRanged)));
                }

                if (damageApplied > 0)
                {
                    // give experience for damage
                    protoWeaponSkill?.ServerOnDamageApplied(playerCharacterSkills,
                                                            damagedObject,
                                                            damageApplied);
                }

                weaponFinalCache.ProtoExplosive?.ServerOnObjectHitByExplosion(damagedObject,
                                                                              damageApplied,
                                                                              weaponFinalCache);

                (weaponFinalCache.ProtoWeapon as ProtoItemMobWeaponNova)?
                    .ServerOnObjectHitByNova(damagedObject,
                                             damageApplied,
                                             weaponFinalCache);
            }

            if (hitCharacters.Count == 0)
            {
                return;
            }

            // display damages on clients in scope of every damaged object
            var observers = new HashSet<ICharacter>();
            using var tempList = Api.Shared.GetTempList<ICharacter>();

            foreach (var hitObject in hitCharacters)
            {
                if (hitObject.WorldObject is ICharacter damagedCharacter
                    && !damagedCharacter.IsNpc)
                {
                    // notify the damaged character
                    observers.Add(damagedCharacter);
                }

                Server.World.GetScopedByPlayers(hitObject.WorldObject, tempList);
                tempList.Clear();
                observers.AddRange(tempList.AsList());
            }

            // add all observers within the sound radius
            var eventNetworkRadius = (byte)Math.Max(
                20,
                Math.Ceiling(1.5 * damageDistanceMax));

            tempList.Clear();
            Server.World.GetCharactersInRadius(positionEpicenter.ToVector2Ushort(),
                                               tempList,
                                               radius: eventNetworkRadius,
                                               onlyPlayers: true);
            observers.AddRange(tempList.AsList());

            if (observers.Count > 0)
            {
                Instance.CallClient(observers,
                                    _ => _.ClientRemote_OnCharactersHitByExplosion(hitCharacters));
            }
        }

        public static IEnumerable<(IWorldObject damagedObject, int indexOffset)>
            SharedEnumerateExplosionBombermanDirectionTilesWithTargets(
                Vector2D positionEpicenter,
                int damageDistanceFullDamage,
                int damageDistanceMax,
                IWorldService world,
                int xOffset,
                int yOffset)
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
                    yield break;
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

                if (damagedObject is null
                    && offsetIndex > damageDistanceFullDamage)
                {
                    // no wall or door there
                    // stop damage propagation
                    yield break;
                }

                yield return (damagedObject, offsetIndex);
            }
        }

        private static void ClientOnCharactersHitByExplosion(IReadOnlyList<WeaponHitData> hitCharacters)
        {
            foreach (var hitData in hitCharacters)
            {
                var protoWorldObject = hitData.FallbackProtoWorldObject;
                var objectMaterial = hitData.FallbackObjectMaterial;
                var hitWorldObject = hitData.WorldObject;
                if (hitWorldObject is not null
                    && !hitWorldObject.IsInitialized)
                {
                    hitWorldObject = null;
                }

                var worldObjectPosition = CalculateWorldObjectPosition(hitWorldObject, hitData);

                // apply some volume variation
                var volume = SoundConstants.VolumeHit;
                volume *= RandomHelper.Range(0.8f, 1.0f);
                var pitch = RandomHelper.Range(0.95f, 1.05f);

                if (hitWorldObject is not null)
                {
                    SoundPresetHitExplosion.PlaySound(
                        objectMaterial,
                        hitWorldObject,
                        volume: volume,
                        pitch: pitch);
                }
                else
                {
                    SoundPresetHitExplosion.PlaySound(
                        objectMaterial,
                        protoWorldObject,
                        worldPosition: worldObjectPosition,
                        volume: volume,
                        pitch: pitch);
                }

                WeaponSystemClientDisplay.ClientAddHitSparks(
                    ExplosionHitSparksPreset,
                    hitData,
                    hitWorldObject,
                    protoWorldObject,
                    worldObjectPosition,
                    projectilesCount: 1,
                    objectMaterial: objectMaterial,
                    randomizeHitPointOffset: true,
                    rotationAngleRad: null,
                    randomRotation: true,
                    drawOrder: DrawOrder.Light);
            }

            static Vector2D CalculateWorldObjectPosition(IWorldObject worldObject, WeaponHitData hitData)
            {
                return worldObject switch
                {
                    IDynamicWorldObject dynamicWorldObject => dynamicWorldObject.Position,
                    IStaticWorldObject                     => worldObject.TilePosition.ToVector2D(),
                    _                                      => hitData.FallbackTilePosition.ToVector2D()
                };
            }
        }

        private static double ServerCalculateDistanceToDamagedObject(Vector2D fromPosition, IWorldObject worldObject)
        {
            switch (worldObject)
            {
                case IDynamicWorldObject dynamicWorldObject:
                    // find the closest dynamicObject position inside the explosion area by checking its hitbox
                    var closestPosition = dynamicWorldObject.PhysicsBody.ClampPointInside(fromPosition,
                        CollisionGroups.HitboxRanged,
                        out var isSuccess);
                    if (!isSuccess)
                    {
                        closestPosition = dynamicWorldObject.Position;
                    }

                    // visualize the closest character position
                    //SharedEditorPhysicsDebugger.ServerSendDebugPhysicsTesting(
                    //    new PointShape(closestCharacterPosition, CollisionGroups.HitboxRanged));

                    return fromPosition.DistanceTo(closestPosition);

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

        private static Func<IWorldObject, Vector2D> ServerExplosionGetDistanceToEpicenter(
            Vector2D positionEpicenter,
            CollisionGroup collisionGroup)
        {
            return testResult => ServerGetClosestPointToExplosionEpicenter(testResult.PhysicsBody,
                                                                           positionEpicenter,
                                                                           collisionGroup)
                                 - positionEpicenter;
        }

        private static Vector2D ServerGetClosestPointToExplosionEpicenter(
            IPhysicsBody physicsBody,
            Vector2D positionEpicenter,
            CollisionGroup collisionGroup)
        {
            if (physicsBody.AssociatedWorldObject?.ProtoWorldObject
                is not IProtoStaticWorldObject protoStaticWorldObject)
            {
                return physicsBody.ClampPointInside(
                    positionEpicenter,
                    collisionGroup,
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
            IWorldObject targetWorldObject,
            CollisionGroup collisionGroup)
        {
            var worldObjectCenter = SharedGetWorldObjectCenter(targetWorldObject);
            var worldObjectPointClosestToCharacter = targetWorldObject.PhysicsBody.ClampPointInside(
                positionEpicenter,
                collisionGroup,
                out var isSuccess);

            if (!isSuccess)
            {
                // the physics body seems to not have the specified collider, let's check for the default collider instead
                worldObjectPointClosestToCharacter = targetWorldObject.PhysicsBody.ClampPointInside(
                    positionEpicenter,
                    CollisionGroups.Default,
                    out _);
            }

            // let's test by casting rays from "fromPosition" (usually it's the character's center) to:
            // 0) world object center
            // 1) world object point closest to the character
            // 2) combined - take X from center, take Y from closest
            // 3) combined - take X from closest, take Y from center
            if (TestHasObstacle(worldObjectCenter)
                && TestHasObstacle(worldObjectPointClosestToCharacter)
                && TestHasObstacle((worldObjectCenter.X, worldObjectPointClosestToCharacter.Y))
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
                    positionEpicenter,
                    toPosition,
                    collisionGroup,
                    sendDebugEvent: true);
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

                    if (ReferenceEquals(testWorldObject, targetWorldObject))
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

                    if (testWorldObject.ProtoWorldObject
                        is IDamageableProtoWorldObject { ObstacleBlockDamageCoef: < 1 })
                    {
                        // damage goes through
                        continue;
                    }

                    // obstacle object on the way
                    return true;
                }

                // no obstacles
                return false;
            }

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

        [RemoteCallSettings(DeliveryMode.ReliableOrdered)]
        private void ClientRemote_OnCharactersHitByExplosion(IReadOnlyList<WeaponHitData> hitCharacters)
        {
            ClientOnCharactersHitByExplosion(hitCharacters);
        }
    }
}