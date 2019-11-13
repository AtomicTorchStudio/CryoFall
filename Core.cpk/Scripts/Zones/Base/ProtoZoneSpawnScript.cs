namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones.Spawn;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public abstract class ProtoZoneSpawnScript : ProtoZoneScript<SpawnConfig>, IZoneScriptWithDefaultConfiguration
    {
        public const int DefaultAreaSpawnAttempsCountPerPreset = SpawnZoneAreaSize * SpawnZoneAreaSize / 8;

        public const int DefaultSpawnMaxSpawnFailedAttemptsInRow = 150;

        public const int InitialSpawnMaxSpawnFailedAttemptsInRow = 300;

        /// <summary>
        /// Important to understand:
        /// Spawning is done in two steps - collection of already existing objects and then spawning.
        /// All world is divided into areas of the specified size.
        /// During spawning we check only the same and nearby areas to check the "padding" constraint of the spawn preset.
        /// This spatial subdivision algorithm dramatically reduces amount of checks and calculations,
        /// but it limits max "padding" distance between objects to 2*specified value.
        /// </summary>
        internal const int SpawnZoneAreaSize = 40;

        private const double DefaultObjectSpawnPadding = 1;

        // Minimal amount of objects per area to use the sector density.
        // If the object preset density is too low (PresetDensity*AreaSize*AreaSize<Threshold)
        // the server will use global spawn algorithm instead.
        private const double SectorDensityThreshold = 1;

        private static readonly ICoreServerService Core = IsServer ? Server.Core : null;

        // ensure that objects will not spawn when player is observing the area
        private static readonly double MaxDistanceToPlayerWhenSpawning
            = Math.Max(SpawnZoneAreaSize / 2.0, ScriptingConstants.PlayerScopeSizeHalf * 1.05);

        private static readonly double MaxDistanceToPlayerWhenSpawningSqr
            = MaxDistanceToPlayerWhenSpawning * MaxDistanceToPlayerWhenSpawning;

        private static readonly Random Random = Api.Random;

        // one for all ProtoZoneSpawnScript instances - used to chain/queue spawn tasks
        private static readonly SemaphoreSlim serverSpawnSemaphore = new SemaphoreSlim(1, 1);

        private static readonly IWorldServerService ServerWorldService = IsServer ? Server.World : null;

        // one per ProtoZoneSpawnScript instance
        private readonly HashSet<CurrentlyExecutingTaskKey> executingEntries = new HashSet<CurrentlyExecutingTaskKey>();

        private bool hasServerOnObjectSpawnedMethodOverride;

        protected ProtoZoneSpawnScript()
        {
            this.DefaultConfiguration = new SpawnConfig(this, densityMultiplier: 1);
        }

        public virtual bool CanSpawnIfPlayersNearby => false;

        public IZoneScriptConfig DefaultConfiguration { get; }

        public override string Name => "Zone spawn script: " + this.ShortId;

        /// <summary>
        /// Gets the list of object prototypes to spawn.
        /// </summary>
        public ObjectSpawnPreset[] SpawnList { get; private set; }

        protected virtual double MaxSpawnAttempsMultiplier => 1;

        public IZoneScriptConfig Configure(double densityMultiplier)
        {
            return new SpawnConfig(this, densityMultiplier);
        }

        public sealed override async void ServerInvoke(SpawnConfig config, IProtoTrigger trigger, IServerZone zone)
        {
            var key = new CurrentlyExecutingTaskKey(config, trigger, zone);
            if (!this.executingEntries.Add(key))
            {
                // cannot schedule new request
                Logger.Warning(
                    "The spawning task is already active - cannot schedule a new spawn task until it's completed");
                return;
            }

            await serverSpawnSemaphore.WaitAsync(Api.CancellationToken);

            try
            {
                Logger.Info(new StringBuilder("Spawn script \"", capacity: 256)
                            .Append(this.ShortId)
                            .Append("\" for zone \"")
                            .Append(zone.ProtoGameObject.ShortId)
                            .Append("\": spawn started"));
                await this.ServerRunSpawnTaskAsync(config, trigger, zone);
            }
            finally
            {
                this.executingEntries.Remove(key);
                serverSpawnSemaphore.Release();
            }
        }

        protected static TProtoTrigger GetTrigger<TProtoTrigger>()
            where TProtoTrigger : ProtoTrigger, new()
        {
            return Api.GetProtoEntity<TProtoTrigger>();
        }

        protected static bool ServerCheckAnyTileCollisions(
            IPhysicsSpace physicsSpace,
            Vector2D centerLocation,
            double radius)
        {
            foreach (var result in physicsSpace.TestCircle(
                centerLocation,
                radius,
                collisionGroup: CollisionGroups.Default,
                sendDebugEvent: false))
            {
                if (result.PhysicsBody.AssociatedProtoTile != null)
                {
                    // collision with a tile found - probably a cliff or water
                    // avoid spawning there
                    return false;
                }
            }

            return true;
        }

        protected sealed override void PrepareProtoZone(Triggers triggers)
        {
            var spawnList = new SpawnList();
            this.PrepareZoneSpawnScript(triggers, spawnList);
            this.SpawnList = spawnList.ToReadOnly();

            this.hasServerOnObjectSpawnedMethodOverride = this.GetType()
                                                              .HasOverride(nameof(this.ServerOnObjectSpawned),
                                                                           isPublic: false);
        }

        protected abstract void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList);

        protected virtual void ServerOnObjectSpawned(IGameObjectWithProto spawnedObject)
        {
        }

        /// <summary>
        /// Server spawn callback (generic).
        /// </summary>
        /// <param name="trigger">Trigger leading to this spawn.</param>
        /// <param name="zone">Server zone instance.</param>
        /// <param name="protoGameObject">Prototype of object to spawn.</param>
        /// <param name="tilePosition">Position to try spawn at.</param>
        protected IGameObjectWithProto ServerSpawn(
            IProtoTrigger trigger,
            IServerZone zone,
            IProtoGameObject protoGameObject,
            Vector2Ushort tilePosition)
        {
            switch (protoGameObject)
            {
                case IProtoStaticWorldObject protoStaticWorldObject:
                    return this.ServerSpawnStaticObject(trigger, zone, protoStaticWorldObject, tilePosition);

                case IProtoCharacterMob protoMob:
                    return this.ServerSpawnMob(trigger, zone, protoMob, tilePosition);

                case IProtoItem protoItem:
                    return this.ServerSpawnItem(trigger, zone, protoItem, tilePosition);

                default:
                    throw new Exception("Server don't know how to spawn this type of object - " + protoGameObject);
            }
        }

        /// <summary>
        /// Server spawn callback for item.
        /// </summary>
        /// <param name="trigger">Trigger leading to this spawn.</param>
        /// <param name="zone">Server zone instance.</param>
        /// <param name="protoItem">Prototype of item to spawn.</param>
        /// <param name="tilePosition">Position to try spawn at.</param>
        protected virtual IGameObjectWithProto ServerSpawnItem(
            IProtoTrigger trigger,
            IServerZone zone,
            IProtoItem protoItem,
            Vector2Ushort tilePosition)
        {
            var container = ObjectGroundItemsContainer.ServerTryGetOrCreateGroundContainerAtTile(
                tilePosition,
                writeWarningsToLog: false);

            if (container == null)
            {
                // cannot spawn item there
                return null;
            }

            return Server.Items.CreateItem(protoItem, container)
                         .ItemAmounts
                         .Keys
                         .FirstOrDefault();
        }

        /// <summary>
        /// Server spawn callback for mob.
        /// </summary>
        /// <param name="trigger">Trigger leading to this spawn.</param>
        /// <param name="zone">Server zone instance.</param>
        /// <param name="protoMob">Prototype of character mob object to spawn.</param>
        /// <param name="tilePosition">Position to try spawn at.</param>
        protected virtual IGameObjectWithProto ServerSpawnMob(
            IProtoTrigger trigger,
            IServerZone zone,
            IProtoCharacterMob protoMob,
            Vector2Ushort tilePosition)
        {
            var worldPosition = tilePosition.ToVector2D();
            if (!ServerCharacterSpawnHelper.IsPositionValidForCharacterSpawn(worldPosition,
                                                                             isPlayer: false))
            {
                // position is not valid for spawning
                return null;
            }

            return Server.Characters.SpawnCharacter(
                protoMob,
                worldPosition);
        }

        /// <summary>
        /// Server spawn callback for static object.
        /// </summary>
        /// <param name="trigger">Trigger leading to this spawn.</param>
        /// <param name="zone">Server zone instance.</param>
        /// <param name="protoStaticWorldObject">Prototype of static object to spawn.</param>
        /// <param name="tilePosition">Position to try spawn at.</param>
        protected virtual IGameObjectWithProto ServerSpawnStaticObject(
            IProtoTrigger trigger,
            IServerZone zone,
            IProtoStaticWorldObject protoStaticWorldObject,
            Vector2Ushort tilePosition)
        {
            foreach (var tileOffset in protoStaticWorldObject.Layout.TileOffsets)
            {
                // ensure that each tile in object layout is inside the zone
                if (tileOffset != Vector2Int.Zero
                    && !zone.IsContainsPosition(tilePosition.AddAndClamp(tileOffset)))
                {
                    // some tile is outside the zone
                    return null;
                }
            }

            if (!protoStaticWorldObject.CheckTileRequirements(
                    tilePosition,
                    character: null,
                    logErrors: false))
            {
                // cannot spawn static object there
                return null;
            }

            var spawnedObject = ServerWorldService.CreateStaticWorldObject(protoStaticWorldObject, tilePosition);
            if (spawnedObject == null)
            {
                // cannot spawn static object there
                return null;
            }

            // if spawned a vegetation - set random growth progress
            if (protoStaticWorldObject is IProtoObjectVegetation protoVegetation)
            {
                double growProgress;
                if (trigger == null
                    || trigger is TriggerWorldInit)
                {
                    // world initialization spawn
                    growProgress = RandomHelper.RollWithProbability(0.6)
                                       ? 1                          // 60% are spawned in full grown state
                                       : Random.Next(0, 11) / 10.0; // other are spawned with random growth progress
                }
                else
                {
                    // spawn saplings
                    growProgress = 0;
                }

                protoVegetation.ServerSetGrowthProgress(spawnedObject, growProgress);
            }

            ServerDecalsDestroyHelper.DestroyAllDecals(tilePosition, protoStaticWorldObject.Layout);
            return spawnedObject;
        }

        private static IEnumerable<SpawnZoneArea> EnumerateAdjacentZoneAreas(
            Vector2Ushort startPosition,
            IReadOnlyDictionary<Vector2Ushort, SpawnZoneArea> spawnZoneAreas)
        {
            const int size = SpawnZoneAreaSize;
            var topLeft = startPosition - new Vector2Int(-size, -size);
            var topCenter = startPosition - new Vector2Int(0,   -size);
            var topRight = startPosition - new Vector2Int(size, -size);

            var middleLeft = startPosition - new Vector2Int(-size, 0);
            var middleCenter = startPosition; // self
            var middleRight = startPosition - new Vector2Int(size, 0);

            var bottomLeft = startPosition - new Vector2Int(-size, size);
            var bottomCenter = startPosition - new Vector2Int(0,   size);
            var bottomRight = startPosition - new Vector2Int(size, size);

            if (spawnZoneAreas.TryGetValue(topLeft, out var r1))
            {
                yield return r1;
            }

            if (spawnZoneAreas.TryGetValue(topCenter, out var r2))
            {
                yield return r2;
            }

            if (spawnZoneAreas.TryGetValue(topRight, out var r3))
            {
                yield return r3;
            }

            if (spawnZoneAreas.TryGetValue(middleLeft, out var r4))
            {
                yield return r4;
            }

            if (spawnZoneAreas.TryGetValue(middleCenter, out var r5))
            {
                yield return r5;
            }

            if (spawnZoneAreas.TryGetValue(middleRight, out var r6))
            {
                yield return r6;
            }

            if (spawnZoneAreas.TryGetValue(bottomLeft, out var r7))
            {
                yield return r7;
            }

            if (spawnZoneAreas.TryGetValue(bottomCenter, out var r8))
            {
                yield return r8;
            }

            if (spawnZoneAreas.TryGetValue(bottomRight, out var r9))
            {
                yield return r9;
            }
        }

        private static bool IsAreaLocalDensityExceeded(
            SpawnRequest spawnRequest,
            ObjectSpawnPreset preset,
            SpawnZoneArea area,
            out double countToSpawnRemains)
        {
            var spawnedObjectsCount = area.WorldObjectsByPreset.Find(preset)?.Count ?? 0;
            var desiredObjectsCount = (int)Math.Round(
                area.ZoneTilesCount * spawnRequest.Density,
                MidpointRounding.AwayFromZero);

            countToSpawnRemains = desiredObjectsCount - spawnedObjectsCount;
            return countToSpawnRemains <= 0;
        }

        private static bool ServerCheckLandClaimAreaPresence(Vector2Ushort spawnPosition, int paddingToLandClaimAreas)
        {
            if (paddingToLandClaimAreas == 0)
            {
                // simply check if the spawn position is claimed
                return LandClaimSystem.SharedIsLandClaimedByAnyone(spawnPosition);
            }

            var bounds = new RectangleInt(x: spawnPosition.X - paddingToLandClaimAreas,
                                          y: spawnPosition.Y - paddingToLandClaimAreas,
                                          width: paddingToLandClaimAreas,
                                          height: paddingToLandClaimAreas);

            return LandClaimSystem.SharedIsLandClaimedByAnyone(bounds);
        }

        private static bool ServerIsAnyPlayerNearby(Vector2Ushort spawnPosition, List<Vector2Ushort> playersPositions)
        {
            foreach (var playerPosition in playersPositions)
            {
                if (playerPosition.TileSqrDistanceTo(spawnPosition)
                    <= MaxDistanceToPlayerWhenSpawningSqr)
                {
                    // player nearby
                    return true;
                }
            }

            return false;
        }

        private static bool ServerIsAnyPlayerNearby(SpawnZoneArea area, List<Vector2Ushort> playersPositions)
        {
            // check 5 locations of the area - 4 corners and center
            var start = area.StartPosition;
            const int size = SpawnZoneAreaSize;
            var topLeft = start;
            var topRight = new Vector2Ushort((ushort)(start.X + size),    start.Y);
            var bottomLeft = new Vector2Ushort(start.X,                   (ushort)(start.Y + size));
            var bottomRight = new Vector2Ushort((ushort)(start.X + size), (ushort)(start.Y + size));
            var center = new Vector2Ushort((ushort)(start.X + size / 2),  (ushort)(start.Y + size / 2));

            return ServerIsAnyPlayerNearby(center,         playersPositions)
                   || ServerIsAnyPlayerNearby(topLeft,     playersPositions)
                   || ServerIsAnyPlayerNearby(topRight,    playersPositions)
                   || ServerIsAnyPlayerNearby(bottomLeft,  playersPositions)
                   || ServerIsAnyPlayerNearby(bottomRight, playersPositions);
        }

        private static bool ServerIsCanSpawn(
            SpawnRequest spawnRequest,
            ObjectSpawnPreset preset,
            IReadOnlyDictionary<Vector2Ushort, SpawnZoneArea> spawnZoneAreas,
            Vector2Ushort spawnPosition,
            IPhysicsSpace physicsSpace,
            out SpawnZoneArea resultSpawnArea,
            out bool isSectorDensityExceeded)
        {
            if (ServerWorldService.GetTile(spawnPosition)
                                  .IsCliffOrSlope)
            {
                // quick discard - don't spawn on cliff or slope
                resultSpawnArea = null;
                isSectorDensityExceeded = false;
                return false;
            }

            var presetPadding = preset.Padding;
            var presetCustomObjectPadding = preset.CustomObjectPadding;

            resultSpawnArea = null;
            var resultSpawnAreaStartPosition = SpawnZoneArea.CalculateStartPosition(spawnPosition);

            foreach (var area in EnumerateAdjacentZoneAreas(resultSpawnAreaStartPosition, spawnZoneAreas))
            {
                foreach (var nearbyPreset in area.WorldObjectsByPreset)
                {
                    var nearbyObjectPreset = nearbyPreset.Key;
                    double padding;

                    if (nearbyObjectPreset != null)
                    {
                        // preset found
                        if (presetCustomObjectPadding.TryGetValue(nearbyObjectPreset, out padding))
                        {
                            // use custom padding value
                        }
                        else
                        {
                            // don't have custom padding
                            padding = Math.Max(presetPadding, nearbyObjectPreset.Padding);
                        }
                    }
                    else
                    {
                        // preset of another object not defined in this spawn list - use default object spawn padding
                        padding = DefaultObjectSpawnPadding;
                    }

                    foreach (var nearbyObjectTilePosition in nearbyPreset.Value)
                    {
                        var distance = spawnPosition.TileSqrDistanceTo(nearbyObjectTilePosition);

                        // Actually using < will be more correct, but it will produce not so nice-looking result
                        // (objects could touch each other on each side).
                        // So we insist objects must don't even touch each other on their
                        // left/up/right/down edges (but the diagonal corners touch is ok).
                        if (distance <= padding * padding)
                        {
                            // too close
                            isSectorDensityExceeded = false;
                            return false;
                        }
                    }
                }

                if (resultSpawnAreaStartPosition == area.StartPosition)
                {
                    resultSpawnArea = area;
                }
            }

            var needToCheckLandClaimPresence = true;
            if (preset.IsContainsOnlyStaticObjects)
            {
                needToCheckLandClaimPresence = !ServerWorldService.GetTile(spawnPosition)
                                                                  .ProtoTile
                                                                  .IsRestrictingConstruction;
            }

            if (needToCheckLandClaimPresence
                && ServerCheckLandClaimAreaPresence(spawnPosition, preset.PaddingToLandClaimAreas))
            {
                // the land is claimed by players
                resultSpawnArea = null;
                isSectorDensityExceeded = false;
                return false;
            }

            if (preset.CustomCanSpawnCheckCallback != null
                && !preset.CustomCanSpawnCheckCallback(physicsSpace, spawnPosition))
            {
                // custom spawn check failed
                resultSpawnArea = null;
                isSectorDensityExceeded = false;
                return false;
            }

            if (resultSpawnArea == null)
            {
                // no area exist (will be created)
                isSectorDensityExceeded = false;
                return true;
            }

            if (!spawnRequest.UseSectorDensity)
            {
                isSectorDensityExceeded = false;
                return true;
            }

            // ensure that the area/sector density is not exceeded for this spawn preset
            isSectorDensityExceeded = IsAreaLocalDensityExceeded(spawnRequest,
                                                                 preset,
                                                                 resultSpawnArea,
                                                                 out var countToSpawnRemains);
            if (isSectorDensityExceeded)
            {
                // already spawned too many objects of the required type in the area
                return false;
            }

            if (countToSpawnRemains < 1)
            {
                // density allows to spawn an extra object of this type with some small probability
                if (!RandomHelper.RollWithProbability(countToSpawnRemains))
                {
                    return false;
                }
            }

            return true;
        }

        // Noise selector is experimental feature to make spawn distribution always the same.
        // For example, if one area was cleaned of objects, the random selector will help us to ensure that objects
        // will be respawned in this area and not in other areas.
        private NoiseSelector CreateTileRandomSelector(double density, ObjectSpawnPreset preset, int desiredCount)
        {
            var seed = (this.Id.GetHashCode() * 397) ^ preset.GetHashCode();
            // we need to use a little bit higher density in our noise selector,
            // otherwise it will be always impossible to spawn the full amount of objects
            density *= 1.065;

            var minDensity = 0.0;
            if (desiredCount < 10)
            {
                // ensure that min density is high enough to allow spawning of these objects
                minDensity = 0.1;
            }

            // clamp density
            density = MathHelper.Clamp(density, minDensity, max: 1);

            return new NoiseSelector(
                from: 0,
                to: density,
                noise: new WhiteNoise(seed: seed));
        }

        private ObjectSpawnPreset FindPreset(IProtoGameObject protoGameObject)
        {
            if (!(protoGameObject is IProtoSpawnableObject protoSpawnableObject))
            {
                // not spawnable objects cannot have presets
                return null;
            }

            foreach (var preset in this.SpawnList)
            {
                if (preset.Contains(protoSpawnableObject))
                {
                    return preset;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all (already populated) areas with their objects inside the zone
        /// </summary>
        private async Task ServerFillSpawnAreasInZoneAsync(
            IServerZone zone,
            IReadOnlyDictionary<Vector2Ushort, SpawnZoneArea> areas,
            Func<Task> callbackYieldIfOutOfTime)
        {
            // this is a heavy method so we will try to yield every 100 objects to reduce the load
            const int defaultCounterToYieldValue = 100;
            var counterToYield = defaultCounterToYieldValue;

            using (var tempList = Api.Shared.GetTempList<IStaticWorldObject>())
            {
                // this check has a problem - it returns only objects strictly inside the zone,
                // but we also need to consider objects nearby the zone for restriction presets
                //await zone.PopulateStaticObjectsInZone(tempList, callbackYieldIfOutOfTime);

                Api.Server.World.GetStaticWorldObjects(tempList);

                foreach (var staticObject in tempList)
                {
                    await YieldIfOutOfTime();
                    if (staticObject.IsDestroyed)
                    {
                        continue;
                    }

                    var position = staticObject.TilePosition;
                    var area = GetArea(position, isMobTrackingEnumeration: false);
                    if (area is null)
                    {
                        continue;
                    }

                    var protoStaticWorldObject = staticObject.ProtoStaticWorldObject;
                    if (!(protoStaticWorldObject is ObjectGroundItemsContainer))
                    {
                        if (protoStaticWorldObject.IsIgnoredBySpawnScripts)
                        {
                            // we don't consider padding to certain objects such as ground decals
                            // (though they still might affect spawn during the tiles check)
                            continue;
                        }

                        // create entry for regular static object
                        var preset = this.FindPreset(protoStaticWorldObject);
                        if (preset != null
                            && preset.Density > 0
                            && !zone.IsContainsPosition(staticObject.TilePosition))
                        {
                            // this object is a part of the preset spawn list but it's not present in the zone
                            // don't consider this object
                            // TODO: this might cause a problem if there is a padding to this object check
                            continue;
                        }

                        area.Add(preset, position);
                        continue;
                    }

                    // ground container object
                    var itemsContainer = ObjectGroundItemsContainer.GetPublicState(staticObject).ItemsContainer;
                    foreach (var item in itemsContainer.Items)
                    {
                        // create entry for each item in the ground container
                        var preset = this.FindPreset(item.ProtoItem);
                        if (preset != null
                            && preset.Density > 0
                            && !zone.IsContainsPosition(staticObject.TilePosition))
                        {
                            // this object is a part of the preset spawn list but it's not present in the zone
                            // don't consider this object
                            continue;
                        }

                        area.Add(preset, position);
                    }
                }
            }

            var mobsTrackingManager = SpawnedMobsTrackingManagersStore.Get(this, zone);
            foreach (var mob in mobsTrackingManager.EnumerateAll())
            {
                await YieldIfOutOfTime();

                var position = mob.TilePosition;
                var area = GetArea(position, isMobTrackingEnumeration: true);
                area?.Add(this.FindPreset(mob.ProtoCharacter), position);
            }

            return;

            SpawnZoneArea GetArea(Vector2Ushort tilePosition, bool isMobTrackingEnumeration)
            {
                var zoneChunkStartPosition = SpawnZoneArea.CalculateStartPosition(tilePosition);
                if (areas.TryGetValue(zoneChunkStartPosition, out var area))
                {
                    return area;
                }

                if (isMobTrackingEnumeration)
                {
                    return null;
                }

                return null;
//                throw new Exception("No zone area found for " + tilePosition);

                //var newArea = new SpawnZoneArea(zoneChunkStartPosition, zoneChunk);
                //areas.Add(newArea);
                //return newArea;
            }

            Task YieldIfOutOfTime()
            {
                if (--counterToYield > 0)
                {
                    return Task.CompletedTask;
                }

                counterToYield = defaultCounterToYieldValue;
                return callbackYieldIfOutOfTime();
            }
        }

        private async Task ServerRunSpawnTaskAsync(SpawnConfig config, IProtoTrigger trigger, IServerZone zone)
        {
            // The spawning algorithm is inspired by random scattering approach described in the article
            // www.voidinspace.com/2013/06/procedural-generation-a-vegetation-scattering-tool-for-unity3d-part-i/ 
            // but we use a quadtree instead of polygon for the spawn zone definition.
            if (zone.IsEmpty)
            {
                if (trigger != null)
                {
                    Logger.Important($"Cannot spawn at {zone} - the zone is empty");
                }
                else
                {
                    // the spawn script is triggered from the editor system
                    Logger.Dev($"Cannot spawn at {zone} - the zone is empty");
                }

                return;
            }

            var isInitialSpawn = trigger == null || trigger is TriggerWorldInit;
            var yieldIfOutOfTime = isInitialSpawn
                                       ? () => Task.CompletedTask
                                       : (Func<Task>)Core.YieldIfOutOfTime;

            await yieldIfOutOfTime();

            var stopwatchTotal = Stopwatch.StartNew();
            var zonePositionsCount = zone.PositionsCount;

            // please note: this operation is super heavy as it collects spawn areas with objects
            // that's why it's made async
            var spawnZoneAreas = await ServerSpawnZoneAreasHelper.ServerGetCachedZoneAreaAsync(zone, yieldIfOutOfTime);
            await this.ServerFillSpawnAreasInZoneAsync(zone,
                                                       spawnZoneAreas,
                                                       yieldIfOutOfTime);
            var maxSpawnFailedAttemptsInRow = isInitialSpawn
                                                  ? InitialSpawnMaxSpawnFailedAttemptsInRow
                                                  : DefaultSpawnMaxSpawnFailedAttemptsInRow;

            maxSpawnFailedAttemptsInRow = (int)Math.Min(int.MaxValue,
                                                        maxSpawnFailedAttemptsInRow * this.MaxSpawnAttempsMultiplier);

            // calculate how many objects are already available
            var spawnedObjectsCount = spawnZoneAreas
                                      .Values
                                      .SelectMany(_ => _.WorldObjectsByPreset)
                                      .GroupBy(_ => _.Key)
                                      .ToDictionary(g => g.Key, g => g.Sum(list => list.Value.Count));

            await yieldIfOutOfTime();

            // calculate how many objects we need to spawn
            using var allSpawnRequests = Api.Shared.WrapInTempList(
                this.SpawnList
                    .Where(preset => preset.Density > 0)
                    .Select(
                        preset =>
                        {
                            var density = preset.Density * config.DensityMultiplier;
                            var desiredCount = (int)(density * zonePositionsCount);
                            var currentCount = spawnedObjectsCount.Find(preset);
                            //if (isInitialSpawn)
                            //{
                            var countToSpawn = Math.Max(0, desiredCount - currentCount);
                            //}

                            // TODO: refactor this to be actually useful with local density
                            //else // if this is an iteration spawn request
                            //{
                            //    // limit count to spawn to match the iteration limit
                            //    var fractionSpawned = Math.Min(currentCount / (double)desiredCount, 1.0);
                            //    var fractionRange = preset.IterationLimitFractionRange;
                            //
                            //    countToSpawn = (int)Math.Ceiling
                            //        (desiredCount * fractionRange.GetByFraction(1 - fractionSpawned));
                            //}

                            if (preset.SpawnLimitPerIteration.HasValue)
                            {
                                countToSpawn = Math.Min(countToSpawn, preset.SpawnLimitPerIteration.Value);
                            }

                            // we're not using this feature
                            NoiseSelector tileRandomSelector = null;
                            // = this.CreateTileRandomSelector(density, preset, desiredCount);

                            var useSectorDensity = preset.PresetUseSectorDensity
                                                   && ((density
                                                        * SpawnZoneAreaSize
                                                        * SpawnZoneAreaSize)
                                                       >= SectorDensityThreshold);

                            return new SpawnRequest(preset,
                                                    desiredCount,
                                                    currentCount,
                                                    countToSpawn,
                                                    density,
                                                    tileRandomSelector,
                                                    useSectorDensity);
                        }));

            var mobsTrackingManager = SpawnedMobsTrackingManagersStore.Get(this, zone);
            using var tempPlayersPositions = Api.Shared.WrapInTempList(
                Server.Characters.EnumerateAllPlayerCharacters(
                          onlyOnline: true,
                          exceptSpectators: true)
                      .Select(p => p.TilePosition));
            var playersPositions = tempPlayersPositions.AsList();

            var physicsSpace = Server.World.GetPhysicsSpace();

            // stage 1: global spawn
            using var activeSpawnRequestsList = Api.Shared.WrapInTempList(
                allSpawnRequests.Where(request => !request.UseSectorDensity
                                                  && request.CountToSpawn > 0));
            while (activeSpawnRequestsList.Count > 0)
            {
                await yieldIfOutOfTime();
                var spawnPosition = zone.GetRandomPosition(Random);
                TrySpawn(spawnPosition,
                         checkPlayersNearby: !this.CanSpawnIfPlayersNearby,
                         out _,
                         out _);
            }

            stopwatchTotal.Stop();
            var timeSpentSpawnGlobal = stopwatchTotal.Elapsed;

            await yieldIfOutOfTime();

            // stage 2: sector spawn
            stopwatchTotal.Restart();
            if (allSpawnRequests.Any(r => r.UseSectorDensity))
            {
                maxSpawnFailedAttemptsInRow = DefaultAreaSpawnAttempsCountPerPreset;
                if (isInitialSpawn)
                {
                    maxSpawnFailedAttemptsInRow *= 16;
                }

                using var areasList = Api.Shared.WrapInTempList(spawnZoneAreas.Values);
                areasList.Shuffle();

                foreach (var area in areasList)
                {
                    await yieldIfOutOfTime();
                    if (activeSpawnRequestsList.Count > 0)
                    {
                        activeSpawnRequestsList.Clear();
                    }

                    foreach (var spawnRequest in allSpawnRequests)
                    {
                        if (!spawnRequest.UseSectorDensity)
                        {
                            continue;
                        }

                        if (IsAreaLocalDensityExceeded(spawnRequest, spawnRequest.Preset, area, out _))
                        {
                            // already spawned too many objects of the required type in the area
                            continue;
                        }

                        activeSpawnRequestsList.Add(spawnRequest);
                        spawnRequest.FailedAttempts = 0;
                    }

                    if (activeSpawnRequestsList.Count == 0)
                    {
                        continue;
                    }

                    if (!this.CanSpawnIfPlayersNearby
                        && ServerIsAnyPlayerNearby(area, playersPositions))
                    {
                        continue;
                    }

                    // make a few attempts to spawn in this area
                    var attempts = activeSpawnRequestsList.Count * DefaultAreaSpawnAttempsCountPerPreset;
                    for (var attempt = 0; attempt < attempts; attempt++)
                    {
                        await yieldIfOutOfTime();
                        var spawnPosition = area.GetRandomPositionInside(zone, Random);
                        TrySpawn(spawnPosition,
                                 checkPlayersNearby: false,
                                 out var spawnRequest,
                                 out var isSectorDensityExceeded);

                        if (isSectorDensityExceeded)
                        {
                            // sector density exceeded for this spawn request
                            activeSpawnRequestsList.Remove(spawnRequest);
                        }

                        if (activeSpawnRequestsList.Count == 0)
                        {
                            // everything for this sector has been spawned
                            break;
                        }
                    }
                }
            }

            stopwatchTotal.Stop();
            var timeSpentSpawnAreas = stopwatchTotal.Elapsed;

            var sb = new StringBuilder("Spawn script \"", capacity: 1024)
                     .Append(this.ShortId)
                     .Append("\" for zone \"")
                     .Append(zone.ProtoGameObject.ShortId)
                     .AppendLine("\": spawn completed. Stats:")
                     .Append("Time spent (including wait time distributed across frames): ")
                     .Append((timeSpentSpawnGlobal + timeSpentSpawnAreas).TotalMilliseconds.ToString("0.#"))
                     .Append("ms (global: ")
                     .Append(timeSpentSpawnGlobal.TotalMilliseconds.ToString("0.#"))
                     .Append("ms, areas: ")
                     .Append(timeSpentSpawnAreas.TotalMilliseconds.ToString("0.#"))
                     .AppendLine("ms)")
                     .AppendLine("Spawned objects:")
                     .AppendLine(
                         "[format: * preset: +spawnedNowCount, currentCount/desiredCount=ratio%, currentDensity%/requiredDensity%]");

            foreach (var request in allSpawnRequests)
            {
                var currentDensity = request.CurrentCount / (double)zonePositionsCount;
                sb.Append("* ")
                  .Append(request.UseSectorDensity ? "(sector) " : ("(global) "))
                  .Append(request.Preset.PrintEntries())
                  .AppendLine(":")
                  .Append("   +")
                  .Append(request.SpawnedCount)
                  .Append(", ")
                  .Append(request.CurrentCount)
                  .Append('/')
                  .Append(request.DesiredCount)
                  .Append('=')
                  // it's normal if we will have NaN if desired count is zero
                  .Append((request.CurrentCount / (double)request.DesiredCount * 100d).ToString("0.##"))
                  .Append("%, ")
                  .Append((currentDensity * 100).ToString("0.###"))
                  .Append("%/")
                  .Append((request.Density * 100).ToString("0.###"))
                  .AppendLine("%");
            }

            Logger.Important(sb);

            void TrySpawn(
                Vector2Ushort spawnPosition,
                bool checkPlayersNearby,
                out SpawnRequest spawnRequest,
                out bool isSectorDensityExceeded)
            {
                spawnRequest = activeSpawnRequestsList.TakeByRandom(Random);
                if (spawnRequest.TileRandomSelector != null
                    && !spawnRequest.TileRandomSelector
                                    .IsMatch(spawnPosition.X, spawnPosition.Y, rangeMultiplier: 1))
                {
                    // the spawn position doesn't satisfy the the random selector
                    isSectorDensityExceeded = false;
                    return;
                }

                var spawnProtoObject = spawnRequest.Preset.GetRandomObjectProto();
                IGameObjectWithProto spawnedObject = null;

                if (ServerIsCanSpawn(
                    spawnRequest,
                    spawnRequest.Preset,
                    spawnZoneAreas,
                    spawnPosition,
                    physicsSpace,
                    out var spawnZoneArea,
                    out isSectorDensityExceeded))
                {
                    if (!checkPlayersNearby
                        || !ServerIsAnyPlayerNearby(spawnPosition, playersPositions))
                    {
                        spawnedObject = this.ServerSpawn(trigger, zone, spawnProtoObject, spawnPosition);
                    }
                }

                if (spawnedObject == null)
                {
                    // cannot spawn
                    if (++spawnRequest.FailedAttempts >= maxSpawnFailedAttemptsInRow)
                    {
                        // too many attempts failed - stop spawning this preset
                        activeSpawnRequestsList.Remove(spawnRequest);
                    }

                    return;
                }

                if (this.hasServerOnObjectSpawnedMethodOverride)
                {
                    try
                    {
                        this.ServerOnObjectSpawned(spawnedObject);
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception(ex);
                    }
                }

                // spawned successfully
                // register object in zone area
                if (spawnZoneArea == null)
                {
                    throw new Exception("Should be impossible");
                }

                spawnZoneArea.Add(spawnRequest.Preset, spawnPosition);

                if (spawnProtoObject is IProtoCharacterMob)
                {
                    mobsTrackingManager.Add((ICharacter)spawnedObject);
                }

                spawnRequest.OnSpawn();
                spawnRequest.FailedAttempts = 0;
                if (spawnRequest.CountToSpawn == 0)
                {
                    activeSpawnRequestsList.Remove(spawnRequest);
                }
            }
        }

        private struct CurrentlyExecutingTaskKey : IEquatable<CurrentlyExecutingTaskKey>
        {
            private readonly SpawnConfig config;

            private readonly IProtoTrigger trigger;

            private readonly IServerZone zone;

            public CurrentlyExecutingTaskKey(SpawnConfig config, IProtoTrigger trigger, IServerZone zone)
            {
                this.config = config;
                this.trigger = trigger;
                this.zone = zone;
            }

            public static bool operator ==(CurrentlyExecutingTaskKey left, CurrentlyExecutingTaskKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(CurrentlyExecutingTaskKey left, CurrentlyExecutingTaskKey right)
            {
                return !left.Equals(right);
            }

            public bool Equals(CurrentlyExecutingTaskKey other)
            {
                return this.config == other.config
                       && this.trigger == other.trigger
                       && this.zone == other.zone;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is CurrentlyExecutingTaskKey && this.Equals((CurrentlyExecutingTaskKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = this.config != null ? this.config.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (this.trigger != null ? this.trigger.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (this.zone != null ? this.zone.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        /// <summary>
        /// Special class containing the spawn area with world objects located in it.
        /// </summary>
        public class SpawnZoneArea
        {
            public readonly Vector2Ushort StartPosition;

            public readonly Dictionary<ObjectSpawnPreset, List<Vector2Ushort>> WorldObjectsByPreset
                = new Dictionary<ObjectSpawnPreset, List<Vector2Ushort>>();

            /// <summary>
            /// Gets the zone tiles count in the area (the area can not fully include the zone tiles).
            /// </summary>
            public readonly int ZoneTilesCount;

            public SpawnZoneArea(Vector2Ushort startPosition, ZoneChunksHelper.ZoneChunkFilledCellsCounter zoneChunk)
            {
                this.StartPosition = startPosition;
                this.ZoneTilesCount = zoneChunk.Count;
            }

            public static Vector2Ushort CalculateStartPosition(Vector2Ushort tilePosition)
            {
                return new Vector2Ushort(
                    (ushort)(SpawnZoneAreaSize * (tilePosition.X / SpawnZoneAreaSize)),
                    (ushort)(SpawnZoneAreaSize * (tilePosition.Y / SpawnZoneAreaSize)));
            }

            public void Add(ObjectSpawnPreset preset, Vector2Ushort position)
            {
                if (preset == null)
                {
                    preset = ObjectSpawnPreset.Empty;
                }

                if (!this.WorldObjectsByPreset.TryGetValue(preset, out var listPositions))
                {
                    listPositions = new List<Vector2Ushort>();
                    this.WorldObjectsByPreset[preset] = listPositions;
                }

                listPositions.Add(position);
            }

            public Vector2Ushort GetRandomPositionInside(IServerZone zone, Random random)
            {
                var start = this.StartPosition;
                Vector2Ushort result;
                do
                {
                    result = new Vector2Ushort(
                        (ushort)random.Next(start.X, start.X + SpawnZoneAreaSize),
                        (ushort)random.Next(start.Y, start.Y + SpawnZoneAreaSize));
                }
                while (!zone.IsContainsPosition(result));

                return result;
            }
        }
    }
}