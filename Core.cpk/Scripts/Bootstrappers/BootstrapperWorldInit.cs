namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    [PrepareOrder(afterType: typeof(BootstrapperServerWorld))]
    [PrepareOrder(afterType: typeof(LandClaimSystem.BootstrapperLandClaimSystem))]
    public class BootstrapperWorldInit : BaseBootstrapper
    {
        public override void ServerInitialize(IServerConfiguration serverConfiguration)
        {
            if (!PveSystem.ServerIsPvE
                && !Api.IsEditor
                && !Api.Shared.IsDebug)
            {
                // invoke players despawn with some small delay because the world objects are not yet loaded
                // (that's the bootstrapper!)
                ServerTimersSystem.AddAction(
                    delaySeconds: 0.1,
                    DespawnIdlePlayersInPvP);

                ServerTimersSystem.AddAction(
                    delaySeconds: 0.2,
                    DespawnIdleVehiclesInPvP);
            }

            // invoke world init with some small delay because the world objects are not yet loaded
            // (that's the bootstrapper!)
            ServerTimersSystem.AddAction(
                delaySeconds: 0.3,
                () => Api.GetProtoEntity<TriggerWorldInit>().OnWorldInit());
        }

        private static void DespawnIdlePlayersInPvP()
        {
            using var playerCharacters = Api.Shared.WrapInTempList(
                Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: false));

            foreach (var character in playerCharacters.AsList())
            {
                var privateState = PlayerCharacter.GetPrivateState(character);
                if (privateState.IsDespawned
                    || PlayerCharacter.GetPublicState(character).IsDead)
                {
                    continue;
                }

                if (LandClaimSystem.SharedIsOwnedLand(character.TilePosition,
                                                      character,
                                                      requireFactionPermission: false,
                                                      out _))
                {
                    // do not despawn as the player is inside the owned land claim area
                    continue;
                }

                var bedObject = privateState.CurrentBedObject;
                if (bedObject is null)
                {
                    continue;
                }

                // teleport character to bed
                TeleportToBed(character, bedObject);
            }
        }

        private static void DespawnIdleVehiclesInPvP()
        {
            var allVehicles = Server.World.GetWorldObjectsOfProto<IProtoVehicle>();
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (IDynamicWorldObject vehicle in allVehicles)
            {
                var publicState = vehicle.GetPublicState<VehiclePublicState>();
                if (publicState.PilotCharacter is not null)
                {
                    continue;
                }

                if (VehicleDespawnSystem.ServerIsVehicleInsideOwnerBase(vehicle))
                {
                    // don't despawn vehicle as it's inside one of its owners base
                    continue;
                }

                VehicleGarageSystem.ServerPutIntoGarage(vehicle);
            }
        }

        private static void TeleportToBed(ICharacter character, IStaticWorldObject bedObject)
        {
            var bedPosition = bedObject.TilePosition.ToVector2D()
                              + bedObject.ProtoStaticWorldObject.Layout.Center;

            var neighborTiles = bedObject.OccupiedTiles
                                         .SelectMany(t => t.EightNeighborTiles)
                                         .Concat(bedObject.OccupiedTiles)
                                         .Distinct()
                                         .ToList();
            neighborTiles.Shuffle();

            var bedTileHeight = bedObject.OccupiedTile.Height;

            neighborTiles.SortBy(t => t.Position.ToVector2D()
                                       .DistanceSquaredTo(bedPosition));
            var physicsSpace = Server.World.GetPhysicsSpace();

            foreach (var neighborTile in neighborTiles)
            {
                if (neighborTile.Height != bedTileHeight)
                {
                    continue;
                }

                var spawnPosition = neighborTile.Position.ToVector2D() + (0.5, 0.5);
                using (var objectsNearby = physicsSpace.TestCircle(
                    spawnPosition,
                    radius: 0.5,
                    collisionGroup: CollisionGroups.Default))
                {
                    if (objectsNearby.Count > 0)
                    {
                        // invalid tile - obstacles
                        continue;
                    }
                }

                if (!LandClaimSystem.SharedIsPositionInsideOwnedOrFreeArea(neighborTile.Position,
                                                                           character,
                                                                           requireFactionPermission: false))
                {
                    // invalid tile - it's claimed by another player
                    continue;
                }

                // valid tile found - respawn here
                // ensure the character has quit the current vehicle
                VehicleSystem.ServerCharacterExitCurrentVehicle(character, force: true);
                if (PlayerCharacter.GetPublicState(character).CurrentVehicle is not null)
                {
                    Logger.Important($"{character} cannot be teleported to bed as it cannot exit the current vehicle");
                    return;
                }

                Server.World.SetPosition(character, spawnPosition);
                Logger.Important($"{character} teleported to bed {bedObject}");
                return;
            }

            Logger.Important($"{character} cannot be teleported to bed {bedObject}");
        }
    }
}