namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    // ReSharper disable SimplifyLinqExpression
    public class ConstructionTileRequirements : IConstructionTileRequirementsReadOnly
    {
        public const string Error_UnsuitableGround_Message_CanBuildOnlyOn =
            "You can only build on:";

        public const string Error_UnsuitableGround_Message_CannotBuilOn =
            "You cannot build on:";

        public const string Error_UnsuitableGround_Title =
            "Unsuitable ground type";

        public const string ErrorCannotBuildInRestrictedArea = "Cannot build in restricted areas.";

        public const string ErrorCannotBuildOnCliffOrSlope = "Cannot build on a cliff or slope.";

        public const string ErrorCannotBuildOnFarmPlot = "Cannot build on farm plot.";

        public const string ErrorCannotBuildOnFloor = "Cannot build on floor.";

        public const string ErrorCreaturesNearby = "There are creatures nearby.";

        public const string ErrorNoFreeSpace = "No free space.";

        public const string ErrorNotSolidGround = "Not a solid ground.";

        public const string ErrorPlayerNearby = "There are other players nearby.";

        public const string ErrorStandingInCell = "You're standing in the cell!";

        public const string ErrorTooCloseToCliffOrSlope = "Cannot build—too close to a cliff or slope.";

        public const string ErrorTooCloseToWater = "Cannot build—too close to water.";

        // don't make it too large otherwise it's really tough to build a base
        // (a chicken somewhere nearby will make players quite unhappy)
        public const double RequirementNoNpcsRadius = 5;

        // quite high to prevent Fortnite-like gameplay with players spamming blueprints
        // but not too much to ensure the player could be seen even staying above or below
        public const double RequirementNoPlayersRadius = 8;

        public static readonly IConstructionTileRequirementsReadOnly BasicRequirements;

        public static readonly IConstructionTileRequirementsReadOnly DefaultForStaticObjects;

        private static readonly CollisionGroup DefaultCollisionGroup = CollisionGroup.GetDefault();

        private static readonly IWorldService WorldService = Api.IsServer
                                                                 ? (IWorldService)Api.Server.World
                                                                 : Api.Client.World;

        /// <summary>
        /// Checks if there is no dynamic objects in the tile.
        /// </summary>
        public static readonly Validator ValidatorNoPhysicsBodyDynamic
            = new Validator(
                ErrorNoFreeSpace,
                c =>
                {
                    return !TileHasAnyPhysicsObjectsWhere(c.Tile,
                                                          CheckIsNotStaticBody,
                                                          CollisionGroups.Default)
                           && !TileHasAnyPhysicsObjectsWhere(c.Tile,
                                                             CheckIsNotStaticBodyAndNoDefaultCollider,
                                                             CollisionGroups.HitboxRanged)
                           && !TileHasAnyPhysicsObjectsWhere(c.Tile,
                                                             CheckIsNotStaticBodyAndNoDefaultCollider,
                                                             CollisionGroups.HitboxMelee);

                    static bool CheckIsNotStaticBody(TestResult t)
                        => !t.PhysicsBody.IsStatic;

                    static bool CheckIsNotStaticBodyAndNoDefaultCollider(TestResult t)
                        => !t.PhysicsBody.IsStatic
                           && !t.PhysicsBody.HasAnyShapeCollidingWithGroup(CollisionGroups.Default);
                });

        /// <summary>
        /// Checks if there is any NPC nearby (circle physics check with radius defined by the constant RequirementNoNpcsRadius).
        /// </summary>
        public static readonly Validator ValidatorNoNpcsAround
            = new Validator(ErrorCreaturesNearby,
                            c =>
                            {
                                if (c.CharacterBuilder == null)
                                {
                                    // don't perform this check if the action is done by the server only
                                    return true;
                                }

                                var physicsSpace = WorldService.GetPhysicsSpace();
                                using var tempList = physicsSpace.TestCircle(
                                    position: c.Tile.Position.ToVector2D() + (0.5, 0.5),
                                    radius: RequirementNoNpcsRadius,
                                    collisionGroup: DefaultCollisionGroup,
                                    sendDebugEvent: false);
                                foreach (var entry in tempList.AsList())
                                {
                                    if (entry.PhysicsBody.AssociatedWorldObject is ICharacter character
                                        && character.IsNpc)
                                    {
                                        // found npc nearby
                                        return false;
                                    }
                                }

                                return true;
                            });

        /// <summary>
        /// Checks if there is any NPC nearby (circle physics check with radius defined by the constant RequirementNoNpcsRadius).
        /// </summary>
        public static readonly Validator ValidatorNoPlayersNearby
            = new Validator(ErrorPlayerNearby,
                            c =>
                            {
                                if (c.CharacterBuilder == null)
                                {
                                    // don't perform this check if the action is done by the server only
                                    return true;
                                }

                                var tilePosition = c.Tile.Position;
                                if (LandClaimSystem.SharedIsOwnedLand(tilePosition, c.CharacterBuilder, out _))
                                {
                                    // don't perform this check if this is the owned land
                                    return true;
                                }

                                var physicsSpace = WorldService.GetPhysicsSpace();
                                using var tempList = physicsSpace.TestCircle(
                                    position: tilePosition.ToVector2D() + (0.5, 0.5),
                                    radius: RequirementNoPlayersRadius,
                                    collisionGroup: DefaultCollisionGroup,
                                    sendDebugEvent: false);
                                foreach (var entry in tempList.AsList())
                                {
                                    if (entry.PhysicsBody.AssociatedWorldObject is ICharacter character
                                        && !character.IsNpc
                                        && !PartySystem.SharedArePlayersInTheSameParty(
                                            c.CharacterBuilder,
                                            character))
                                    {
                                        // found player nearby and they're not a party member
                                        return false;
                                    }
                                }

                                return true;
                            });

        /// <summary>
        /// (Client only) Checks if there is no current player in the cell.
        /// </summary>
        public static readonly Validator ValidatorClientOnlyNoCurrentPlayer
            = new Validator(ErrorStandingInCell,
                            c =>
                            {
                                if (Api.IsServer)
                                {
                                    // this is a client-only check
                                    return true;
                                }

                                var currentPlayerCharacter = Api.Client.Characters.CurrentPlayerCharacter;
                                return !TileHasAnyPhysicsObjectsWhere(c.Tile,
                                                                      t => currentPlayerCharacter
                                                                           == t.PhysicsBody.AssociatedWorldObject);
                            });

        /// <summary>
        /// Checks if there is no static objects in the tile.
        /// </summary>
        public static readonly Validator ValidatorNoPhysicsBodyStatic
            = new Validator(ErrorNoFreeSpace,
                            c => !TileHasAnyPhysicsObjectsWhere(
                                     c.Tile,
                                     t => t.PhysicsBody.IsStatic
                                          // allow destroyed walls physics in the tile
                                          && !(t.PhysicsBody.AssociatedWorldObject
                                                ?.ProtoWorldObject is ObjectWallDestroyed)));

        public static readonly Validator ValidatorNoStaticObjectsExceptFloor
            = new Validator(ErrorNoFreeSpace,
                            c => c.Tile.StaticObjects.All(
                                o => o.ProtoStaticWorldObject.Kind == StaticObjectKind.Floor
                                     || o.ProtoStaticWorldObject.Kind == StaticObjectKind.FloorDecal));

        public static readonly Validator ValidatorNoStaticObjectsExceptPlayersStructures
            = new Validator(ErrorNoFreeSpace,
                            c => c.Tile.StaticObjects.All(
                                o => o.ProtoStaticWorldObject is IProtoObjectStructure
                                     || o.ProtoStaticWorldObject.Kind == StaticObjectKind.FloorDecal));

        public static readonly Validator ValidatorNoFarmPlot
            = new Validator(ErrorCannotBuildOnFarmPlot,
                            c => !c.Tile.StaticObjects.Any(
                                     o => o.ProtoStaticWorldObject is IProtoObjectFarmPlot));

        public static readonly Validator ValidatorNoFloor
            = new Validator(ErrorCannotBuildOnFloor,
                            c => !c.Tile.StaticObjects.Any(
                                     o => o.ProtoStaticWorldObject.Kind == StaticObjectKind.Floor));

        public static readonly Validator ValidatorNotRestrictedArea
            = new Validator(ErrorCannotBuildInRestrictedArea,
                            c =>
                            {
                                if (Api.IsClient)
                                {
                                    // check skipped on client
                                    return true;
                                }

                                if (c.CharacterBuilder == null)
                                {
                                    // check skipped if there is no character context
                                    // (so scripts can spawn objects in restricted areas)
                                    return true;
                                }

                                return !ServerZoneRestrictedArea.Value.IsContainsPosition(c.Tile.Position);
                            });

        public static readonly Validator ValidatorNotRestrictedAreaEvenForServer
            = new Validator(ErrorCannotBuildInRestrictedArea,
                            c =>
                            {
                                if (Api.IsClient)
                                {
                                    // check skipped on client
                                    return true;
                                }

                                return !ServerZoneRestrictedArea.Value.IsContainsPosition(c.Tile.Position);
                            });

        public static readonly IConstructionTileRequirementsReadOnly DefaultForPlayerStructuresOwnedLand;

        public static readonly IConstructionTileRequirementsReadOnly DefaultForPlayerStructuresOwnedOrFreeLand;

        private static readonly Lazy<IServerZone> ServerZoneRestrictedArea
            = new Lazy<IServerZone>(
                () => ZoneSpecialConstructionRestricted.Instance.ServerZoneInstance);

        private readonly List<Validator> checkFunctions = new List<Validator>();

        static ConstructionTileRequirements()
        {
            BasicRequirements = new ConstructionTileRequirements()
                                .Add(ErrorNotSolidGround,
                                     c => c.Tile.ProtoTile.Kind == TileKind.Solid)
                                .Add(ErrorCannotBuildInRestrictedArea,
                                     c => c.CharacterBuilder == null
                                          || !c.Tile.ProtoTile.IsRestrictingConstruction)
                                .Add(ErrorCannotBuildOnCliffOrSlope,
                                     c => !c.Tile.IsCliffOrSlope)
                                .Add(ErrorTooCloseToCliffOrSlope,
                                     c => c.Tile.EightNeighborTiles.All(neighbor => !neighbor.IsCliffOrSlope))
                                .Add(ErrorTooCloseToWater,
                                     c => c.Tile.EightNeighborTiles.All(
                                         neighbor => neighbor.ProtoTile.Kind != TileKind.Water))
                                .Add(LandClaimSystem.ValidatorCheckLandClaimDepositCooldown)
                                .Add(ObjectMineralPragmiumSource.ValidatorCheckNoPragmiumSourceNearbyOnPvE);

            DefaultForStaticObjects = BasicRequirements
                                      .Clone()
                                      .Add(ValidatorNoFarmPlot)
                                      .Add(ValidatorNoStaticObjectsExceptFloor)
                                      // ensure no physical objects (static/dynamic) at tile
                                      .Add(ValidatorNoPhysicsBodyStatic)
                                      .AddClientOnly(ValidatorClientOnlyNoCurrentPlayer)
                                      .Add(ValidatorNoPhysicsBodyDynamic);

            DefaultForPlayerStructuresOwnedOrFreeLand = DefaultForStaticObjects
                                                        .Clone()
                                                        .Add(ValidatorNotRestrictedArea)
                                                        .Add(ValidatorNoNpcsAround)
                                                        .Add(ValidatorNoPlayersNearby)
                                                        .Add(LandClaimSystem.ValidatorIsOwnedOrFreeLand)
                                                        .Add(LandClaimSystem.ValidatorNoRaid);

            DefaultForPlayerStructuresOwnedLand = DefaultForStaticObjects
                                                  .Clone()
                                                  .Add(ValidatorNotRestrictedArea)
                                                  .Add(ValidatorNoNpcsAround)
                                                  .Add(ValidatorNoPlayersNearby)
                                                  .Add(LandClaimSystem.ValidatorIsOwnedLand)
                                                  .Add(LandClaimSystem.ValidatorNoRaid);
        }

        public ConstructionTileRequirements(ConstructionTileRequirements toClone)
        {
            this.Add(toClone);
        }

        public ConstructionTileRequirements()
        {
        }

        public delegate bool DelegateCheck(Context context);

        public static bool TileHasAnyPhysicsObjectsWhere(
            Tile tile,
            Func<TestResult, bool> check,
            CollisionGroup collisionGroup = null)
        {
            var physicsSpace = WorldService.GetPhysicsSpace();
            return Test(collisionGroup ?? DefaultCollisionGroup);

            bool Test(CollisionGroup checkCollisionGroup)
            {
                using var tempList = physicsSpace.TestRectangle(
                    // include some padding, otherwise the check will include border-objects
                    position: tile.Position.ToVector2D() + (0.01, 0.01),
                    size: (0.98, 0.98),
                    collisionGroup: checkCollisionGroup,
                    sendDebugEvent: false);
                foreach (var entry in tempList.AsList())
                {
                    if (check(entry))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public ConstructionTileRequirements Add(string errorMessage, DelegateCheck checkFunc)
        {
            this.checkFunctions.Add(new Validator(errorMessage, checkFunc));
            return this;
        }

        public ConstructionTileRequirements Add(Validator validator)
        {
            if (validator.Function == null
                || string.IsNullOrEmpty(validator.ErrorMessage))
            {
                throw new ArgumentNullException(
                    nameof(validator),
                    "Incorrect validator - don't have a function or errorMessage");
            }

            this.checkFunctions.Add(validator);
            return this;
        }

        public ConstructionTileRequirements Add(ConstructionTileRequirements existingRequirements)
        {
            foreach (var checkFunction in existingRequirements.checkFunctions)
            {
                this.checkFunctions.Add(checkFunction);
            }

            return this;
        }

        public ConstructionTileRequirements Add(IConstructionTileRequirementsReadOnly existingRequirements)
        {
            return this.Add((ConstructionTileRequirements)existingRequirements);
        }

        public bool Check(
            IProtoStaticWorldObject proto,
            Vector2Ushort startTilePosition,
            ICharacter character,
            bool logErrors)
        {
            foreach (var tileOffset in proto.Layout.TileOffsets)
            {
                var tile = WorldService.GetTile(startTilePosition.X + tileOffset.X,
                                                startTilePosition.Y + tileOffset.Y,
                                                logOutOfBounds: false);
                var context = new Context(tile, character, proto, tileOffset);
                string errorMessage;

                if (tile.IsOutOfBounds)
                {
                    errorMessage = "Out of bounds";
                }
                else
                {
                    if (this.Check(context, out errorMessage))
                    {
                        // valid tile
                        continue;
                    }
                }

                // check failed
                if (!logErrors)
                {
                    return false;
                }

                if (Api.IsServer)
                {
                    Api.Logger.Warning(
                        $"Cannot place {proto} at {startTilePosition} - check failed:"
                        + Environment.NewLine
                        + errorMessage);
                }

                ConstructionSystem.SharedShowCannotPlaceNotification(
                    character,
                    errorMessage,
                    proto);

                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Check(Context context, out string errorMessage)
        {
            foreach (var function in this.checkFunctions)
            {
                if (!function.Function(context))
                {
                    errorMessage = function.ErrorMessage;
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        public ConstructionTileRequirements Clear()
        {
            this.checkFunctions.Clear();
            return this;
        }

        public ConstructionTileRequirements Clone()
        {
            return new ConstructionTileRequirements(this);
        }

        private ConstructionTileRequirements AddClientOnly(Validator validator)
        {
            if (Api.IsClient)
            {
                this.Add(validator);
            }

            return this;
        }

        public struct Context
        {
            [NonSerialized]
            public readonly ICharacter CharacterBuilder;

            [NonSerialized]
            public readonly IProtoStaticWorldObject ProtoStaticObjectToBuild;

            [NonSerialized]
            public readonly Tile Tile;

            [NonSerialized]
            public readonly Vector2Int TileOffset;

            public Context(
                Tile tile,
                ICharacter characterBuilder,
                IProtoStaticWorldObject protoStaticObjectToBuild,
                Vector2Int tileOffset)
            {
                this.TileOffset = tileOffset;
                this.CharacterBuilder = characterBuilder;
                this.ProtoStaticObjectToBuild = protoStaticObjectToBuild;
                this.Tile = tile;
            }
        }

        [NotPersistent]
        public class Validator
        {
            public readonly DelegateCheck Function;

            private readonly Func<string> errorMessageFunc;

            private string errorMessage;

            public Validator(string errorMessage, DelegateCheck function)
            {
                this.Function = function;
                this.errorMessage = errorMessage;
                this.errorMessageFunc = null;
            }

            public Validator(Func<string> errorMessageFunc, DelegateCheck function)
            {
                this.Function = function;
                this.errorMessageFunc = errorMessageFunc;
            }

            public string ErrorMessage => this.errorMessage ??= this.errorMessageFunc();

            public override string ToString()
            {
                return this.ErrorMessage;
            }
        }
    }
}