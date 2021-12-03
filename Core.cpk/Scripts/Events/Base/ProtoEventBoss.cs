namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoEventBoss
        : ProtoEventWithArea<
            EventBossPrivateState,
            EventBossPublicState,
            EventBossClientState>
    {
        // {0} is the boss name
        public const string Notification_VictoryAnnouncement_Format =
            "{0} has been defeated by:";

        protected ProtoEventBoss()
        {
            this.ServerSpawnZones = new Lazy<IReadOnlyList<IServerZone>>(
                () =>
                {
                    Api.ValidateIsServer();
                    return this.ServerSetupSpawnZones();
                });
        }

        public virtual ushort AreaBarrierRadius => 24;

        public override ushort AreaRadius => 55;

        public override bool ConsolidateNotifications => false;

        public override TimeSpan EventDuration
            => this.EventDurationWithoutDelay + this.EventStartDelayDuration;

        public abstract TimeSpan EventDurationWithoutDelay { get; }

        public abstract TimeSpan EventStartDelayDuration { get; }

        public override TimeSpan EventStartPostponeDurationFrom { get; }
            = TimeSpan.FromMinutes(10);

        public override TimeSpan EventStartPostponeDurationTo { get; }
            = TimeSpan.FromMinutes(30);

        public IReadOnlyList<IProtoSpawnableObject> SpawnPreset { get; private set; }

        public virtual ushort SpawnRadiusMax => 20;

        protected Lazy<IReadOnlyList<IServerZone>> ServerSpawnZones { get; }

        public override void ClientDeinitialize(ILogicObject gameObject)
        {
            base.ClientDeinitialize(gameObject);
            this.ClientDestroyBossAreaBarrier(gameObject);
        }

        public sealed override bool ServerIsTriggerAllowed(ProtoTrigger trigger)
        {
            if (this.ServerIsSameEventExist())
            {
                Logger.Error("The same event is already running, cannot start a new one: " + this);
                return false;
            }

            if (this.ServerSpawnZones.Value.All(z => z.IsEmpty))
            {
                Logger.Error("All zones are empty (not mapped in the world), no place to start the event: " + this);
                return false;
            }

            return this.ServerIsTriggerAllowedForBossEvent(trigger);
        }

        public abstract bool ServerIsTriggerAllowedForBossEvent(ProtoTrigger trigger);

        public double SharedGetTimeRemainsToEventStart(EventWithAreaPublicState publicState)
        {
            var time = IsServer
                           ? Server.Game.FrameTime
                           : Client.CurrentGame.ServerFrameTimeApproximated;
            var eventCreatedTime = publicState.EventEndTime - this.EventDuration.TotalSeconds;
            var timeSinceCreation = time - eventCreatedTime;
            var result = this.EventStartDelayDuration.TotalSeconds - timeSinceCreation;
            return Math.Max(result, 0);
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            base.ClientUpdate(data);

            if (this.SharedGetTimeRemainsToEventStart(data.PublicState) > 0)
            {
                this.ClientCreateBossAreaBarrier(data.GameObject);
            }
            else
            {
                this.ClientDestroyBossAreaBarrier(data.GameObject);
            }
        }

        protected override bool ServerCreateEventSearchArea(
            IWorldServerService world,
            Vector2Ushort eventPosition,
            ushort circleRadius,
            out Vector2Ushort circlePosition)
        {
            circlePosition = eventPosition;
            return true;
        }

        protected override void ServerInitializeEvent(ServerInitializeData data)
        {
            data.PrivateState.Init();
        }

        protected override bool ServerIsValidEventPosition(Vector2Ushort tilePosition)
        {
            return true;
        }

        protected virtual bool ServerIsValidSpawnPosition(Vector2D worldPosition)
        {
            const double noObstaclesCheckRadius = 1.5;
            var physicsSpace = Server.World.GetPhysicsSpace();
            foreach (var _ in physicsSpace.TestCircle(worldPosition,
                                                      radius: noObstaclesCheckRadius,
                                                      CollisionGroups.Default,
                                                      sendDebugEvent: false).EnumerateAndDispose())
            {
                // position is not valid for spawning
                return false;
            }

            return ServerCharacterSpawnHelper.IsValidSpawnTile(
                Api.Server.World.GetTile(worldPosition.ToVector2Ushort()),
                checkNeighborTiles: true);
        }

        protected virtual void ServerOnBossEventStarted(ILogicObject worldEvent)
        {
        }

        protected override void ServerOnEventDestroyed(ILogicObject worldEvent)
        {
            // destroy all the spawned objects
            foreach (var spawnedObject in GetPrivateState(worldEvent).SpawnedWorldObjects)
            {
                if (!spawnedObject.IsDestroyed)
                {
                    Server.World.DestroyObject(spawnedObject);
                }
            }
        }

        protected sealed override void ServerOnEventWithAreaStarted(ILogicObject worldEvent)
        {
            this.ServerOnBossEventStarted(worldEvent);
        }

        protected override Vector2Ushort ServerPickEventPosition(ILogicObject worldEvent)
        {
            var stopwatch = Stopwatch.StartNew();

            // select a random boss spawn zone
            var zoneInstance = this.ServerSpawnZones.Value.TakeByRandom();

            var chunkPositions = new HashSet<Vector2Ushort>(capacity: 50 * 50);
            var positionToCheck = new Stack<Vector2Ushort>();

            // perform the location selection several times to determine the possible locations
            var possibleLocations = new Dictionary<Vector2Ushort, uint>();
            for (var i = 0; i < 15; i++)
            {
                chunkPositions.Clear();
                positionToCheck.Clear();

                // select a random position inside the selected zone
                var randomPosition = zoneInstance.GetRandomPosition(RandomHelper.Instance);

                // use fill flood to locate all the positions
                // within the continuous area around the selected random position
                positionToCheck.Push(randomPosition);
                FillFlood();

                // calculate the center position of the area
                Vector2Ushort result = ((ushort)chunkPositions.Average(c => c.X),
                                        (ushort)chunkPositions.Average(c => c.Y));

                if (!possibleLocations.ContainsKey(result))
                {
                    possibleLocations.Add(result, 0);
                }

                possibleLocations[result]++;

                void FillFlood()
                {
                    while (positionToCheck.Count > 0)
                    {
                        var pos = positionToCheck.Pop();
                        if (pos.X == 0
                            || pos.Y == 0
                            || pos.X >= ushort.MaxValue
                            || pos.Y >= ushort.MaxValue)
                        {
                            // reached the bound - not enclosed
                            continue;
                        }

                        if (!chunkPositions.Add(pos))
                        {
                            // already visited
                            continue;
                        }

                        if (!zoneInstance.IsContainsPosition(pos))
                        {
                            continue;
                        }

                        positionToCheck.Push(((ushort)(pos.X - 1), pos.Y));
                        positionToCheck.Push(((ushort)(pos.X + 1), pos.Y));
                        positionToCheck.Push((pos.X, (ushort)(pos.Y - 1)));
                        positionToCheck.Push((pos.X, (ushort)(pos.Y + 1)));
                    }
                }
            }

            //Logger.Dev("Possible boss spawn locations: "
            //           + possibleLocations.Select(p => p.Key + ": " + p.Value).GetJoinedString(Environment.NewLine));

            // each location has equal weight
            var selectedLocation = possibleLocations.Keys.TakeByRandom();
            Logger.Important(
                $"[Stopwatch] Selecting the boss event position took: {stopwatch.Elapsed.TotalMilliseconds:F1} ms");
            return selectedLocation;
        }

        protected abstract void ServerPrepareBossEvent(Triggers triggers, List<IProtoSpawnableObject> spawnPreset);

        protected sealed override void ServerPrepareEvent(Triggers triggers)
        {
            var list = new List<IProtoSpawnableObject>();
            this.ServerPrepareBossEvent(triggers, list);
            Api.Assert(list.Count > 0, "Spawn preset cannot be empty");
            this.SpawnPreset = list;
        }

        protected virtual IReadOnlyList<IServerZone> ServerSetupSpawnZones()
        {
            var result = new List<IServerZone>();
            AddZone(Api.GetProtoEntity<ZoneEventBoss>());

            void AddZone(IProtoZone zone)
            {
                var instance = zone.ServerZoneInstance;
                result.Add(instance);
            }

            return result;
        }

        protected virtual void ServerSpawnBossEventObjects(
            ILogicObject worldEvent,
            Vector2D circlePosition,
            ushort circleRadius,
            List<IWorldObject> spawnedObjects)
        {
            foreach (var protoObjectToSpawn in this.SpawnPreset)
            {
                TrySpawn();

                void TrySpawn()
                {
                    const int maxAttempts = 3000;
                    var attempt = 0;

                    do
                    {
                        // select random position inside the circle
                        // (the circle is expanded proportionally by the number of attempts performed)
                        var spawnPosition =
                            SharedCircleLocationHelper.SharedSelectRandomPositionInsideTheCircle(
                                circlePosition,
                                this.SpawnRadiusMax * (attempt / (double)maxAttempts));

                        if (!this.ServerIsValidSpawnPosition(spawnPosition))
                        {
                            // doesn't match any specific checks determined by the inheritor (such as a zone test)
                            continue;
                        }

                        var spawnedObject = Server.Characters.SpawnCharacter((IProtoCharacter)protoObjectToSpawn,
                                                                             spawnPosition);
                        spawnedObjects.Add(spawnedObject);
                        Logger.Important($"Spawned world object: {spawnedObject} for world event {worldEvent}");

                        if (spawnedObject.ProtoGameObject is IProtoCharacterMob protoCharacterMob)
                        {
                            protoCharacterMob.ServerSetSpawnState(spawnedObject,
                                                                  MobSpawnState.Spawning);
                        }

                        break;
                    }
                    while (++attempt < maxAttempts);

                    if (attempt == maxAttempts)
                    {
                        Logger.Error($"Cannot spawn world object: {protoObjectToSpawn} for world event {worldEvent}");
                    }
                }
            }
        }

        protected override void ServerTryFinishEvent(ILogicObject worldEvent)
        {
            var canFinish = true;

            var spawnedWorldObjects = GetPrivateState(worldEvent).SpawnedWorldObjects;
            for (var index = spawnedWorldObjects.Count - 1; index >= 0; index--)
            {
                var spawnedObject = spawnedWorldObjects[index];
                if (spawnedObject.IsDestroyed)
                {
                    spawnedWorldObjects.RemoveAt(index);
                    continue;
                }

                if (Server.World.IsObservedByAnyPlayer(spawnedObject))
                {
                    // use despawn animation
                    if (spawnedObject.ProtoGameObject is IProtoCharacterMob protoCharacterMob
                        && !spawnedObject.GetPublicState<CharacterMobPublicState>().IsDead)
                    {
                        protoCharacterMob.ServerSetSpawnState((ICharacter)spawnedObject,
                                                              MobSpawnState.Despawning);
                    }
                }
                else
                {
                    Server.World.DestroyObject(spawnedObject);
                    spawnedWorldObjects.RemoveAt(index);
                    continue;
                }

                // still has a spawned object which cannot be destroyed as it's observed by a player
                canFinish = false;
                break;
            }

            if (canFinish)
            {
                base.ServerTryFinishEvent(worldEvent);
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var activeEvent = data.GameObject;
            var privateState = GetPrivateState(activeEvent);
            var publicState = GetPublicState(activeEvent);

            var timeRemainsToEventStart = this.SharedGetTimeRemainsToEventStart(publicState);
            if (timeRemainsToEventStart > 0)
            {
                // the boss event is not yet started
                // ensure the barrier exist
                this.ServerCreateBossAreaBarrier(data.GameObject);
                return;
            }

            this.ServerDestroyBossAreaBarrier(data.GameObject);

            if (!privateState.IsSpawnCompleted)
            {
                privateState.IsSpawnCompleted = true;

                // time to spawn
                this.ServerSpawnBossEventObjects(activeEvent,
                                                 publicState.AreaCirclePosition.ToVector2D(),
                                                 publicState.AreaCircleRadius,
                                                 privateState.SpawnedWorldObjects);

                if (privateState.SpawnedWorldObjects.Count == 0)
                {
                    Logger.Error($"Incorrect boss event spawn: {activeEvent} - destroying it immediately");
                    Server.World.DestroyObject(activeEvent);
                }

                return;
            }

            var countDestroyed = 0;
            var totalCount = privateState.SpawnedWorldObjects.Count;

            foreach (var spawnedObject in privateState.SpawnedWorldObjects)
            {
                if (spawnedObject.IsDestroyed)
                {
                    countDestroyed++;
                }
            }

            if (countDestroyed == totalCount)
            {
                this.ServerTryFinishEvent(activeEvent);
            }
        }

        private void ClientCreateBossAreaBarrier(ILogicObject worldEvent)
        {
            var publicState = GetPublicState(worldEvent);
            var clientState = GetClientState(worldEvent);
            if (clientState.BarrierPhysicsBody is not null)
            {
                return;
            }

            clientState.BarrierPhysicsBody = this.SharedCreateBarrierPhysicsBody(worldEvent);

            // create barrier visualizer
            var position = publicState.AreaCirclePosition;
            var ellipse = new Ellipse()
            {
                Width = 2 * this.AreaBarrierRadius * ScriptingConstants.TileSizeVirtualPixels,
                Height = 2 * this.AreaBarrierRadius * ScriptingConstants.TileSizeVirtualPixels,
                StrokeThickness = 6,
                Stroke = new SolidColorBrush(Api.Client.UI.GetApplicationResource<Color>("ColorAlt5").WithAlpha(0xEE)),
                Fill = new RadialGradientBrush()
                {
                    GradientStops = new GradientStopCollection()
                    {
                        new()
                        {
                            Color = Api.Client.UI.GetApplicationResource<Color>("ColorAlt2").WithAlpha(0xAA),
                            Offset = 1
                        },
                        new()
                        {
                            Color = Api.Client.UI.GetApplicationResource<Color>("ColorAlt2").WithAlpha(0x00),
                            Offset = 0.5
                        }
                    }
                }
            };

            var sceneObjectVisualizer = Client.Scene.CreateSceneObject("Boss event area: " + worldEvent,
                                                                       position.ToVector2D() + (0.5, 0.5));
            var attachedControl = Api.Client.UI.AttachControl(
                sceneObjectVisualizer,
                positionOffset: (0, 0),
                uiElement: ellipse,
                isFocusable: false,
                isScaleWithCameraZoom: true);
            attachedControl.SetCustomZIndex(-1);

            clientState.SceneObjectBarrierVisualizer = sceneObjectVisualizer;
        }

        private void ClientDestroyBossAreaBarrier(ILogicObject gameObject)
        {
            var clientState = GetClientState(gameObject);
            if (clientState.BarrierPhysicsBody is not null)
            {
                Client.World.DestroyStandalonePhysicsBody(clientState.BarrierPhysicsBody);
                clientState.BarrierPhysicsBody = null;
            }

            if (clientState.SceneObjectBarrierVisualizer is { } sceneObjectVisualizer)
            {
                clientState.SceneObjectBarrierVisualizer = null;

                // instead of instantly removing the barrier, fade-out quickly
                var durationFadeOut = 2.5;
                sceneObjectVisualizer.Destroy(delay: durationFadeOut);

                var control = sceneObjectVisualizer.FindComponent<IComponentAttachedControl>().Control;
                var anim = new DoubleAnimation
                {
                    From = 1.0,
                    To = (double)0,
                    Duration = new Duration(TimeSpan.FromSeconds(durationFadeOut)),
                    EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn }
                };

                Storyboard.SetTarget(anim, control);
                Storyboard.SetTargetProperty(anim, new PropertyPath(UIElement.OpacityProperty.Name));
                var storyboard = new Storyboard();
                storyboard.Children.Add(anim);
                storyboard.Begin(control);
            }
        }

        /// <summary>
        /// Barrier (a circle impenetrable area) is present only in PvE to prevent players from rushing
        /// into the boss area before the boss is spawned.
        /// </summary>
        private void ServerCreateBossAreaBarrier(ILogicObject worldEvent)
        {
            var publicState = GetPublicState(worldEvent);
            if (publicState.ServerBarrierPhysicsBody is not null)
            {
                return;
            }

            // create the barrier
            publicState.ServerBarrierPhysicsBody = this.SharedCreateBarrierPhysicsBody(worldEvent);

            var activeEventPosition = publicState.AreaCirclePosition;
            var barrierRadius = this.AreaBarrierRadius + 1;
            if (barrierRadius > byte.MaxValue)
            {
                Logger.Error(
                    "The boss area circle should never have a radius larger than 255 tiles as it prevents players despawn in PvE: "
                    + worldEvent);
                barrierRadius = byte.MaxValue;
            }

            var barrierRadiusSqr = barrierRadius * barrierRadius;

            // despawn all player characters inside the barrier or very close to it
            using var tempCharacters = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetCharactersInRadius(activeEventPosition,
                                               tempCharacters,
                                               (byte)barrierRadius,
                                               onlyPlayers: true);

            foreach (var character in tempCharacters.AsList())
            {
                CharacterDespawnSystem.DespawnCharacter(character);
            }

            // despawn all vehicles inside the barrier or very close to it
            using var tempListVehicles = Api.Shared.GetTempList<IDynamicWorldObject>();
            foreach (var protoVehicle in Api.FindProtoEntities<IProtoVehicle>())
            {
                tempListVehicles.Clear();
                protoVehicle.GetAllGameObjects(tempListVehicles.AsList());
                foreach (var vehicle in tempListVehicles.AsList())
                {
                    if (vehicle.TilePosition.TileSqrDistanceTo(activeEventPosition)
                        <= barrierRadiusSqr)
                    {
                        VehicleGarageSystem.ServerPutIntoGarage(vehicle);
                    }
                }
            }

            // Please note: NPC characters that are stuck inside the area are pushed outside it by the physics engine
        }

        private void ServerDestroyBossAreaBarrier(ILogicObject worldEvent)
        {
            var publicState = GetPublicState(worldEvent);
            if (publicState.ServerBarrierPhysicsBody is not null)
            {
                Server.World.DestroyStandalonePhysicsBody(publicState.ServerBarrierPhysicsBody);
                publicState.ServerBarrierPhysicsBody = null;
            }
        }

        private IPhysicsBody SharedCreateBarrierPhysicsBody(ILogicObject worldEvent)
        {
            IWorldService world = IsClient
                                      ? Client.World
                                      : Server.World;
            var publicState = GetPublicState(worldEvent);
            var physicsBody = world.CreateStandalonePhysicsBody(publicState.AreaCirclePosition.ToVector2D()
                                                                + (0.5, 0.5));
            physicsBody.AddShapeCircle(this.AreaBarrierRadius);
            world.AddStandalonePhysicsBody(physicsBody, world.GetPhysicsSpace());
            return physicsBody;
        }
    }
}