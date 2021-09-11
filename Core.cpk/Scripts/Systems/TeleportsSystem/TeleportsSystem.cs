namespace AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class TeleportsSystem : ProtoSystem<TeleportsSystem>
    {
        public const double TeleportationAnimationDuration = 1.5;

        /// <summary>
        /// The delay is necessary to apply the full screen fade out effect for current character
        /// after it has disappeared.
        /// </summary>
        public const double TeleportationDelay = 1.5;

        private const string DatabaseKeyPlayerDiscoveredTeleports = "PlayerDiscoveredTeleports";

        private const int PayWithBloodHealthAdditionalAbsolute = 10;

        private const float PayWithBloodHealthFraction = 0.5f;

        // players need to come as close as this distance in order to discover the teleport
        private const double ServerTeleportDiscoverDistance = 10;

        public static readonly SuperObservableCollection<Vector2Ushort> ClientDiscoveredTeleports
            = new();

        private static readonly HashSet<IStaticWorldObject> ServerTeleports
            = IsServer
                  ? new HashSet<IStaticWorldObject>()
                  : null;

        private static Dictionary<ICharacter, HashSet<Vector2Ushort>> serverPlayerDiscoveredTeleports;

        public static event Action<IStaticWorldObject, ICharacter> ServerTeleportDiscovered;

        public static IProtoItem OptionalTeleportationItemProto
            => Api.GetProtoEntity<ItemPragmiumHeart>();

        public static IReadOnlyCollection<IStaticWorldObject> ServerAllTeleports => ServerTeleports;

        [NotLocalizable]
        public override string Name => "Teleport system";

        public static Task<bool> ClientIsTeleportHasUnfriendlyPlayersNearby(Vector2Ushort targetTeleportPosition)
        {
            return Instance.CallServer(
                _ => _.ServerRemote_IsTeleportHasUnfriendlyPlayersNearby(targetTeleportPosition));
        }

        public static void ClientTeleport(Vector2Ushort targetTeleportPosition, bool payWithItem)
        {
            if (!ClientDiscoveredTeleports.Contains(targetTeleportPosition))
            {
                throw new Exception("Doesn't have a discovered teleport " + targetTeleportPosition);
            }

            var characterCurrentStats = PlayerCharacter.GetPublicState(ClientCurrentCharacterHelper.Character)
                                                       .CurrentStats;
            var hpCost = SharedCalculateTeleportationBloodCost(characterCurrentStats);
            if (!payWithItem)
            {
                if (characterCurrentStats.HealthCurrent <= hpCost)
                {
                    throw new Exception("Health is too low");
                }
            }

            Instance.CallServer(
                _ => _.ServerRemote_Teleport(targetTeleportPosition, payWithItem));
            Logger.Important("Requested teleportation to " + targetTeleportPosition);
        }

        public static void ServerAddTeleportToDiscoveredList(ICharacter character, IStaticWorldObject objectTeleport)
        {
            if (objectTeleport.ProtoGameObject is not ProtoObjectTeleport protoObjectTeleport)
            {
                throw new Exception($"Not a {nameof(ProtoObjectTeleport)}: {objectTeleport}");
            }

            var teleportPosition = objectTeleport.TilePosition;
            if (!serverPlayerDiscoveredTeleports.TryGetValue(character,
                                                             out var characterDiscoveredTeleports))
            {
                characterDiscoveredTeleports = new HashSet<Vector2Ushort>();
                serverPlayerDiscoveredTeleports[character] = characterDiscoveredTeleports;
            }

            if (!characterDiscoveredTeleports.Add(teleportPosition))
            {
                // the teleport is already discovered
                return;
            }

            Logger.Info("Teleport discovered: " + objectTeleport, character);
            Instance.CallClient(character,
                                _ => _.ClientRemote_DiscoveredTeleport(teleportPosition, protoObjectTeleport));

            Api.SafeInvoke(
                () => ServerTeleportDiscovered?.Invoke(objectTeleport, character));
        }

        public static void ServerDiscoverAllTeleports(ICharacter character)
        {
            Api.Assert(!character.IsNpc, "Must be a player character");

            if (!serverPlayerDiscoveredTeleports.TryGetValue(character,
                                                             out var characterDiscoveredTeleports))
            {
                characterDiscoveredTeleports = new HashSet<Vector2Ushort>();
                serverPlayerDiscoveredTeleports[character] = characterDiscoveredTeleports;
            }

            var handler = ServerTeleportDiscovered;
            foreach (var objectTeleport in ServerTeleports)
            {
                if (!characterDiscoveredTeleports.Add(objectTeleport.TilePosition))
                {
                    // the teleport is already discovered
                    continue;
                }

                if (handler is not null)
                {
                    Api.SafeInvoke(
                        () => handler.Invoke(objectTeleport, character));
                }
            }

            Logger.Important("All teleports discovered", character);
            Instance.ServerSyncDiscoveredTeleportList(character);
        }

        public static IReadOnlyCollection<Vector2Ushort> ServerGetDiscoveredTeleports(ICharacter character)
        {
            return serverPlayerDiscoveredTeleports.TryGetValue(character, out var result)
                       ? (IReadOnlyCollection<Vector2Ushort>)result
                       : Array.Empty<Vector2Ushort>();
        }

        public static void ServerRegisterTeleport(IStaticWorldObject gameObject)
        {
            ServerTeleports.Add(gameObject);
        }

        public static void ServerUnregisterTeleport(IStaticWorldObject gameObject)
        {
            if (!ServerTeleports.Remove(gameObject))
            {
                return;
            }

            var teleportPosition = gameObject.TilePosition;
            foreach (var pair in serverPlayerDiscoveredTeleports)
            {
                if (!pair.Value.Remove(teleportPosition))
                {
                    continue;
                }

                var character = pair.Key;
                if (!character.ServerIsOnline)
                {
                    continue;
                }

                Instance.ServerSyncDiscoveredTeleportList(character);
            }
        }

        public static int SharedCalculateTeleportationBloodCost(ICharacter character)
        {
            return SharedCalculateTeleportationBloodCost(
                PlayerCharacter.GetPublicState(character)
                               .CurrentStats);
        }

        public static int SharedCalculateTeleportationBloodCost(CharacterCurrentStats characterCurrentStats)
        {
            return (int)(PayWithBloodHealthAdditionalAbsolute
                         + (int)characterCurrentStats.HealthCurrent * PayWithBloodHealthFraction);
        }

        public static bool SharedHasOptionalRequiredItem(ICharacter character)
        {
            var containers = new CharacterContainersProvider(character, includeEquipmentContainer: false);
            return containers.ContainsItemsOfType(OptionalTeleportationItemProto,
                                                  requiredCount: 1);
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            Server.Characters.PlayerOnlineStateChanged += this.ServerPlayerOnlineStateChangedHandler;
            Server.World.WorldBoundsChanged += ServerWorldBoundsChanged;

            TriggerEveryFrame.ServerRegister(this.ServerUpdate,
                                             $"{nameof(TeleportsSystem)}.{nameof(this.ServerUpdate)}");

            if (Server.Database.TryGet(nameof(TeleportsSystem),
                                       DatabaseKeyPlayerDiscoveredTeleports,
                                       out serverPlayerDiscoveredTeleports))
            {
                return;
            }

            ServerRecreateTeleportsStorage();
        }

        private static IDynamicWorldObject ClientGetGameObjectByEntry(
            (uint id, GameObjectType objectType, Vector2Ushort tilePosition) entry)
        {
            var worldObject = entry.objectType switch
            {
                GameObjectType.Character
                    => Client.World.GetGameObjectById<ICharacter>(GameObjectType.Character,
                                                                  entry.id),
                GameObjectType.DynamicObject // vehicle 
                    => Client.World.GetGameObjectById<IDynamicWorldObject>(GameObjectType.DynamicObject,
                                                                           entry.id),
                _ => throw new ArgumentOutOfRangeException()
            };
            return worldObject;
        }

        private static IStaticWorldObject ServerGetTargetTeleportForCharacter(
            ICharacter character,
            Vector2Ushort targetTeleportPosition,
            out IStaticWorldObject currentTeleport)
        {
            currentTeleport = InteractionCheckerSystem.SharedGetCurrentInteraction(character)
                                  as IStaticWorldObject;
            if (currentTeleport?.ProtoGameObject is not ProtoObjectTeleport)
            {
                currentTeleport = null;
                throw new Exception("Not interacting with any teleport object");
            }

            if (currentTeleport.TilePosition == targetTeleportPosition)
            {
                throw new Exception("Cannot teleport to the current teleport location");
            }

            if (!serverPlayerDiscoveredTeleports.TryGetValue(character,
                                                             out var discoveredTeleportPositions)
                || !discoveredTeleportPositions.Contains(targetTeleportPosition))
            {
                throw new Exception("Doesn't have a discovered teleport " + targetTeleportPosition);
            }

            var targetTeleport = ServerTeleports.First(t => t.TilePosition == targetTeleportPosition);
            if (ReferenceEquals(targetTeleport, currentTeleport))
            {
                throw new Exception("Cannot teleport to the current teleport");
            }

            return targetTeleport;
        }

        private static void ServerRecreateTeleportsStorage()
        {
            ServerTeleports.Clear();

            serverPlayerDiscoveredTeleports = new Dictionary<ICharacter, HashSet<Vector2Ushort>>();
            Server.Database.Set(nameof(TeleportsSystem),
                                DatabaseKeyPlayerDiscoveredTeleports,
                                serverPlayerDiscoveredTeleports);
        }

        private static Vector2D ServerSelectTeleportTargetPosition(
            Vector2D startPosition,
            Size2F rectangleSize)
        {
            var physicsSpace = Server.World.GetPhysicsSpace();

            var radiuses = new (double min, double max)[]
            {
                // format: (min, max) distances
                (0, 0),
                (0, 0.5),
                (0.5, 1.0),
                (1, 2),
                (2, 3),
                (3, 4),
                (4, 10)
            };

            var halfRectangleSize = rectangleSize / 2.0f;

            for (var index = 0; index < radiuses.Length; index++)
            {
                var attemptsRemains = index * 6;
                var radius = radiuses[index];

                do
                {
                    var offset = radius.min + RandomHelper.NextDouble() * (radius.max - radius.min);
                    var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
                    var selectedPosition = new Vector2D(startPosition.X + offset * Math.Cos(angle),
                                                        startPosition.Y + offset * Math.Sin(angle));

                    using (var objectsNearby = physicsSpace.TestRectangle(
                        selectedPosition - halfRectangleSize,
                        rectangleSize,
                        collisionGroup: CollisionGroups.Default,
                        sendDebugEvent: Api.IsEditor))
                    {
                        if (objectsNearby.Count > 0)
                        {
                            // invalid tile - obstacles
                            continue;
                        }
                    }

                    // ensure that the tile is valid
                    var isValidTile = true;
                    var spawnTile = Server.World.GetTile(selectedPosition.ToVector2Ushort());
                    foreach (var neighborTile in spawnTile.EightNeighborTiles.ConcatOne(spawnTile))
                    {
                        if (!neighborTile.IsValidTile
                            || neighborTile.ProtoTile.Kind != TileKind.Solid)
                        {
                            isValidTile = false;
                            break;
                        }
                    }

                    if (!isValidTile)
                    {
                        continue;
                    }

                    // valid tile found - teleport here
                    return selectedPosition;
                }
                while (attemptsRemains-- > 0);
            }

            return default;
        }

        private static bool ServerTeleportPlayerTo(
            ICharacter character,
            Vector2D targetTeleportPosition)
        {
            targetTeleportPosition = ServerSelectTeleportTargetPosition(
                targetTeleportPosition,
                // TODO: remove this hardcoded size for player character
                rectangleSize: (0.45, 0.25));
            if (targetTeleportPosition == default)
            {
                Logger.Error($"Unable to teleport character to {targetTeleportPosition}: no free space", character);
                return false;
            }

            Server.World.SetPosition(character, targetTeleportPosition);
            Logger.Important("Character teleported to " + targetTeleportPosition, character);

            return true;
        }

        private static bool ServerTeleportVehicle(Vector2D targetTeleportPosition, IDynamicWorldObject vehicle)
        {
            if (vehicle.GetPublicState<VehiclePublicState>().PilotCharacter
                    is not null)
            {
                // the vehicle has a pilot
                return false;
            }

            try
            {
                targetTeleportPosition = ServerSelectTeleportTargetPosition(
                    targetTeleportPosition,
                    // TODO: remove this hardcoded size
                    // currently all vehicles are smaller than this size,
                    // but we may have larger vehicles in the future updates
                    rectangleSize: (1.1, 0.5));
                if (targetTeleportPosition == default)
                {
                    Logger.Error($"Unable to teleport vehicle to {targetTeleportPosition}: no free space)");
                    return false;
                }

                Server.World.SetPosition(vehicle, targetTeleportPosition);
                Logger.Important($"Teleported {vehicle} to {targetTeleportPosition}");

                // update the last dismounted vehicle position
                var vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();
                if (vehiclePrivateState.ServerLastPilotCharacter is { } lastPilot)
                {
                    PlayerCharacter.GetPrivateState(lastPilot).LastDismountedVehicleMapMark
                        = new LastDismountedVehicleMapMark(vehicle);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Cannot teleport a vehicle: " + vehicle);
                return false;
            }
        }

        private static void ServerWorldBoundsChanged()
        {
            ServerRecreateTeleportsStorage();
        }

        private void ClientRemote_DiscoveredTeleport(
            Vector2Ushort teleportPosition,
            ProtoObjectTeleport protoObjectTeleport)
        {
            ClientDiscoveredTeleports.Add(teleportPosition);
            Logger.Important("Discovered a teleport: " + teleportPosition);

            NotificationSystem.ClientShowNotification(
                title: protoObjectTeleport.Name,
                message: CoreStrings.Teleport_Discovered,
                color: NotificationColor.Good,
                icon: protoObjectTeleport.Icon);
        }

        private void ClientRemote_DiscoveredTeleportsList([CanBeNull] List<Vector2Ushort> worldPositions)
        {
            if (worldPositions is not null)
            {
                ClientDiscoveredTeleports.ClearAndAddRange(worldPositions);
                Logger.Important("Discovered teleport positions: " + worldPositions.GetJoinedString());
            }
            else
            {
                ClientDiscoveredTeleports.Clear();
                Logger.Important("Don't have any discovered teleport positions");
            }
        }

        private void ClientRemote_ObjectsTeleportation(
            List<(uint id, GameObjectType objectType, Vector2Ushort tilePosition)> objectsToTeleport,
            bool isTeleportationOut)
        {
            foreach (var entry in objectsToTeleport)
            {
                ClientComponentTeleportationEffect.CreateEffect(
                    ClientGetGameObjectByEntry(entry),
                    entry.tilePosition,
                    TeleportationAnimationDuration,
                    isTeleportationOut ? TeleportationDelay : 0,
                    isTeleportationOut);
            }
        }

        private void ServerPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
        {
            this.ServerSyncDiscoveredTeleportList(character);
        }

        /// <summary>
        /// For PvP servers: checks whether there any players nearby the teleport
        /// (in such case player is warned about their presence to prevent teleporting
        /// into a place that may have campers).
        /// </summary>
        [RemoteCallSettings(timeInterval: 1)]
        private bool ServerRemote_IsTeleportHasUnfriendlyPlayersNearby(Vector2Ushort targetTeleportPosition)
        {
            if (PveSystem.ServerIsPvE)
            {
                // no sense in requesting this data on PvE servers
                return false;
            }

            var character = ServerRemoteContext.Character;
            var targetTeleport = ServerGetTargetTeleportForCharacter(character, targetTeleportPosition, out _);

            using var tempListCharacters = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(targetTeleport, tempListCharacters);
            tempListCharacters.Remove(character);

            if (tempListCharacters.Count == 0)
            {
                // no player characters near the teleport 
                return false;
            }

            // determine if there is at least a single non-friendly character 
            var friendlyCharacterNames = new HashSet<string>();
            foreach (var memberName in PartySystem.ServerGetPartyMembersReadOnly(character))
            {
                friendlyCharacterNames.Add(memberName);
            }

            var faction = FactionSystem.ServerGetFaction(character);
            if (faction is not null)
            {
                foreach (var memberName in FactionSystem.ServerGetFactionMemberNames(faction))
                {
                    friendlyCharacterNames.Add(memberName);
                }
            }

            foreach (var otherCharacter in tempListCharacters.AsList())
            {
                if (friendlyCharacterNames.Contains(otherCharacter.Name))
                {
                    continue;
                }

                if (faction is not null)
                {
                    if (FactionSystem.SharedGetFactionDiplomacyStatus(faction,
                                                                      FactionSystem.SharedGetClanTag(otherCharacter))
                        == FactionDiplomacyStatus.Ally)
                    {
                        // ignore an alliance member on the other side
                        continue;
                    }
                }

                // found a non-friendly character near the teleport
                return true;
            }

            return false;
        }

        [RemoteCallSettings(timeInterval: 2)]
        private void ServerRemote_Teleport(Vector2Ushort targetTeleportPosition, bool payWithItem)
        {
            var character = ServerRemoteContext.Character;
            // Please note: the interaction validation with the current teleport is done by this method
            var targetTeleport = ServerGetTargetTeleportForCharacter(character,
                                                                     targetTeleportPosition,
                                                                     out var currentTeleport);
            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            if (characterPrivateState.CurrentActionState is not null)
            {
                return;
            }

            var characterPublicState = PlayerCharacter.GetPublicState(character);
            var characterCurrentStats = characterPublicState.CurrentStats;

            if (!payWithItem)
            {
                var hpCost = SharedCalculateTeleportationBloodCost(characterCurrentStats);
                if (characterCurrentStats.HealthCurrent <= hpCost)
                {
                    throw new Exception("Not enough HP to use the teleport");
                }
            }

            var vehiclesToTeleport = ((ProtoObjectTeleport)currentTeleport.ProtoGameObject)
                .ServerGetVehiclesToTeleport(currentTeleport, character);

            var tempObserversTeleportOut = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(currentTeleport, tempObserversTeleportOut.AsList());

            // notify clients about the teleportation start
            var objectsToTeleport = new List<(uint id, GameObjectType objectType, Vector2Ushort tilePosition)>();
            objectsToTeleport.Add((character.Id, GameObjectType.Character, character.TilePosition));
            foreach (var vehicle in vehiclesToTeleport)
            {
                objectsToTeleport.Add((vehicle.Id, GameObjectType.DynamicObject, vehicle.TilePosition));
            }

            Instance.CallClient(tempObserversTeleportOut.AsList(),
                                _ => _.ClientRemote_ObjectsTeleportation(objectsToTeleport, true));

            InteractionCheckerSystem.SharedAbortCurrentInteraction(character);

            // block player movement and prevent calling teleportation again while under delay
            characterPrivateState.SetCurrentActionState(new CharacterTeleportAction(character));

            // teleport after the animation and delay
            ServerTimersSystem.AddAction(
                TeleportationAnimationDuration + TeleportationDelay,
                () =>
                {
                    if (characterPrivateState.CurrentActionState is not CharacterTeleportAction)
                    {
                        return;
                    }

                    characterPrivateState.SetCurrentActionState(null);

                    if (character.GetPublicState<ICharacterPublicState>().IsDead)
                    {
                        Logger.Warning("Cannot teleport a dead character (died during teleportation): " + character);
                        return;
                    }

                    if (character.GetPublicState<PlayerCharacterPublicState>().CurrentVehicle is not null)
                    {
                        Logger.Warning("Cannot teleport a player in vehicle: " + character);
                        return;
                    }

                    // calculate the actual HP cost (as it may have changed during the delay)
                    var hpCost = SharedCalculateTeleportationBloodCost(characterCurrentStats);
                    if (!payWithItem)
                    {
                        if (characterCurrentStats.HealthCurrent <= hpCost)
                        {
                            throw new Exception("Not enough HP to use the teleport");
                        }
                    }

                    var teleportedObjects =
                        new List<(uint id, GameObjectType objectType, Vector2Ushort tilePosition)>();
                    if (!ServerTeleportPlayerTo(character,
                                                targetTeleportPosition.ToVector2D()
                                                + targetTeleport.ProtoStaticWorldObject.Layout.Center))
                    {
                        return;
                    }

                    teleportedObjects.Add((character.Id, GameObjectType.Character, character.TilePosition));
                    try
                    {
                        var targetTeleportPosition1 = targetTeleportPosition.ToVector2D()
                                                      + targetTeleport.ProtoStaticWorldObject.Layout.Center;
                        foreach (var vehicle in vehiclesToTeleport)
                        {
                            if (ServerTeleportVehicle(targetTeleportPosition1, vehicle))
                            {
                                teleportedObjects.Add((vehicle.Id, GameObjectType.DynamicObject, vehicle.TilePosition));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception(ex, "Cannot teleport vehicles");
                    }

                    if (payWithItem)
                    {
                        if (!SharedHasOptionalRequiredItem(character))
                        {
                            throw new Exception("Don't have a required item");
                        }

                        var containers = new CharacterContainersProvider(
                            character,
                            includeEquipmentContainer: false);
                        Server.Items.DestroyItemsOfType(containers,
                                                        OptionalTeleportationItemProto,
                                                        countToDestroy: 1,
                                                        out var destroyedCount);
                        if (destroyedCount == 0)
                        {
                            throw new Exception("Don't have a required item");
                        }
                    }
                    else // pay with blood, deduct the HP
                    {
                        characterCurrentStats.ServerSetHealthCurrent(
                            characterCurrentStats.HealthCurrent - hpCost);
                    }

                    var tempObserversTeleportIn = Api.Shared.GetTempList<ICharacter>();
                    Server.World.GetScopedByPlayers(targetTeleport, tempObserversTeleportIn.AsList());

                    Instance.CallClient(tempObserversTeleportIn.AsList(),
                                        _ => _.ClientRemote_ObjectsTeleportation(teleportedObjects, false));
                });
        }

        private void ServerSyncDiscoveredTeleportList(ICharacter character)
        {
            if (!character.ServerIsOnline)
            {
                return;
            }

            if (!serverPlayerDiscoveredTeleports.TryGetValue(character, out var discoveredTeleportPositions)
                || discoveredTeleportPositions.Count == 0)
            {
                this.CallClient(character,
                                _ => _.ClientRemote_DiscoveredTeleportsList(null));
                return;
            }

            using var tempList = Api.Shared.GetTempList<Vector2Ushort>();
            var worldPositions = tempList.AsList();
            foreach (var worldPosition in discoveredTeleportPositions)
            {
                tempList.Add(worldPosition);
            }

            this.CallClient(character,
                            _ => _.ClientRemote_DiscoveredTeleportsList(worldPositions));
        }

        private void ServerUpdate()
        {
            var spreadInterval = 2; // one update per 2 seconds per teleport
            var frameSpread = spreadInterval * Server.Game.FrameRate;
            var frameNumber = Server.Game.FrameNumber;

            using var tempListPlayers = Api.Shared.GetTempList<ICharacter>();
            var listPlayers = tempListPlayers.AsList();

            foreach (var objectTeleport in ServerTeleports)
            {
                if ((frameNumber + objectTeleport.Id) % frameSpread != 0)
                {
                    // don't check discovery of this teleport this frame
                    continue;
                }

                Server.World.GetScopedByPlayers(objectTeleport, listPlayers);
                if (listPlayers.Count == 0)
                {
                    continue;
                }

                var teleportPosition = objectTeleport.TilePosition;
                foreach (var character in listPlayers)
                {
                    if (character.ProtoGameObject.GetType() != typeof(PlayerCharacter))
                    {
                        continue;
                    }

                    if (character.TilePosition.TileSqrDistanceTo(teleportPosition)
                        > ServerTeleportDiscoverDistance * ServerTeleportDiscoverDistance)
                    {
                        // the player is too far from the teleport to discover it
                        continue;
                    }

                    ServerAddTeleportToDiscoveredList(character, objectTeleport);
                }
            }
        }
    }
}