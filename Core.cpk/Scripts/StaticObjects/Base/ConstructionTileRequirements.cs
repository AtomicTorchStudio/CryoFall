namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    // ReSharper disable SimplifyLinqExpression
    public class ConstructionTileRequirements : IConstructionTileRequirementsReadOnly
    {
        public const string Error_UnsuitableGround_Message_CanBuildOnlyOn =
            "You can only build on:";

        public const string Error_UnsuitableGround_Message_CannotBuilOn =
            "You cannot build on:";

        public const string Error_UnsuitableGround_Title =
            "Unsuitable ground type";

        public const string ErrorCannotBuildEventNearby = "Cannot build—too close to the event location.";

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
                                if (c.CharacterBuilder is null)
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

        public static readonly Validator ValidatorSameHeightLevelAsPlayer
            = new Validator(CoreStrings.Notification_TooFar,
                            c =>
                            {
                                if (c.CharacterBuilder is null)
                                {
                                    // don't perform this check if the action is done by the server only
                                    return true;
                                }

                                return c.Tile.Height
                                       == c.CharacterBuilder.Tile.Height;
                            });

        /// <summary>
        /// Checks if there is any NPC nearby (circle physics check with radius defined by the constant RequirementNoNpcsRadius).
        /// </summary>
        public static readonly Validator ValidatorNoPlayersNearby
            = new Validator(ErrorPlayerNearby,
                            c =>
                            {
                                if (c.CharacterBuilder is null)
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

                                if (c.CharacterBuilder is null)
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

        public static readonly IConstructionTileRequirementsReadOnly DefaultForPlayerStructures;

        public static readonly IConstructionTileRequirementsReadOnly DefaultForPlayerStructuresOwnedOrFreeLand;

        private static readonly Lazy<IServerZone> ServerZoneRestrictedArea
            = new Lazy<IServerZone>(
                () => ZoneSpecialConstructionRestricted.Instance.ServerZoneInstance);

        public static readonly Validator ValidatorSolidGround
            = new Validator(ErrorNotSolidGround,
                            c => c.Tile.ProtoTile.Kind == TileKind.Solid);

        public static readonly Validator ValidatorNotCliffOrSlope
            = new Validator(ErrorCannotBuildOnCliffOrSlope,
                            c => !c.Tile.IsCliffOrSlope);

        public static readonly Validator ValidatorCheckNoEventObjectNearby
            = new Validator(
                ErrorCannotBuildEventNearby,
                context =>
                {
                    var forCharacter = context.CharacterBuilder;
                    if (forCharacter is null)
                    {
                        return true;
                    }

                    if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                    {
                        return true;
                    }

                    var position = context.Tile.Position;
                    var world = Api.IsServer
                                    ? (IWorldService)Api.Server.World
                                    : (IWorldService)Api.Client.World;

                    var eventObjects = world.GetStaticWorldObjectsOfProto<IProtoObjectEventEntry>();
                    var maxDistanceSqr = LandClaimSystem.MaxLandClaimSize.Value / 2;
                    maxDistanceSqr *= maxDistanceSqr;

                    foreach (var eventObject in eventObjects)
                    {
                        if (position.TileSqrDistanceTo(eventObject.TilePosition)
                            <= maxDistanceSqr)
                        {
                            // too close to an event object
                            return false;
                        }
                    }

                    return true;
                });

        private readonly List<Validator> checkFunctions = new List<Validator>();

        static ConstructionTileRequirements()
        {
            BasicRequirements = new ConstructionTileRequirements()
                                .Add(ValidatorSolidGround)
                                .Add(ErrorCannotBuildInRestrictedArea,
                                     c => c.CharacterBuilder is null
                                          || !c.Tile.ProtoTile.IsRestrictingConstruction)
                                .Add(ValidatorNotCliffOrSlope)
                                .Add(ErrorTooCloseToCliffOrSlope,
                                     c => c.Tile.EightNeighborTiles.All(neighbor => !neighbor.IsCliffOrSlope))
                                .Add(ErrorTooCloseToWater,
                                     c => c.Tile.EightNeighborTiles.All(
                                         neighbor => neighbor.ProtoTile.Kind != TileKind.Water))
                                .Add(ValidatorSameHeightLevelAsPlayer)
                                .Add(LandClaimSystem.ValidatorCheckLandClaimDepositCooldown)
                                .Add(ObjectMineralPragmiumSource.ValidatorCheckNoPragmiumSourceNearbyOnPvE)
                                .Add(ValidatorCheckNoEventObjectNearby);

            DefaultForStaticObjects = BasicRequirements
                                      .Clone()
                                      .Add(ValidatorNoFarmPlot)
                                      .Add(ValidatorNoStaticObjectsExceptFloor)
                                      .AddClientOnly(ValidatorClientOnlyNoCurrentPlayer)
                                      .Add(ValidatorNoPhysicsBodyDynamic);

            var defaultForStructures = DefaultForStaticObjects
                                       .Clone()
                                       .Add(ValidatorNotRestrictedArea)
                                       .Add(ValidatorNoNpcsAround)
                                       .Add(ValidatorNoPlayersNearby);

            DefaultForPlayerStructuresOwnedOrFreeLand
                = defaultForStructures.Clone()
                                      .Add(LandClaimSystem.ValidatorIsOwnedOrFreeLand)
                                      .Add(LandClaimSystem.ValidatorNoRaid)
                                      .Add(LandClaimSystem.ValidatorNoShieldProtection);

            DefaultForPlayerStructures
                = defaultForStructures.Clone()
                                      .Add(LandClaimSystem.ValidatorIsOwnedLandInPvEOnly)
                                      .Add(LandClaimSystem.ValidatorNoRaid)
                                      .Add(LandClaimSystem.ValidatorNoShieldProtection);
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
            if (validator.Function is null
                || !validator.HasErrorMessage)
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

        public ConstructionTileRequirements AddClientOnly(Validator validator)
        {
            if (Api.IsClient)
            {
                this.Add(validator);
            }

            return this;
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
                string errorMessage;

                if (tile.IsOutOfBounds)
                {
                    errorMessage = "Out of bounds";
                }
                else
                {
                    var context = new Context(tile,
                                              character,
                                              proto,
                                              tileOffset,
                                              startTilePosition);
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

        public readonly struct Context
        {
            [NonSerialized]
            public readonly ICharacter CharacterBuilder;

            [NonSerialized]
            [CanBeNull]
            public readonly IStaticWorldObject ObjectToRelocate;

            [NonSerialized]
            public readonly IProtoStaticWorldObject ProtoStaticObjectToBuild;

            [NonSerialized]
            public readonly Vector2Ushort StartTilePosition;

            [NonSerialized]
            public readonly Tile Tile;

            [NonSerialized]
            public readonly Vector2Int TileOffset;

            public Context(
                Tile tile,
                ICharacter characterBuilder,
                IProtoStaticWorldObject protoStaticObjectToBuild,
                Vector2Int tileOffset,
                Vector2Ushort startTilePosition,
                IStaticWorldObject objectToRelocate = null)
            {
                this.Tile = tile;
                this.CharacterBuilder = characterBuilder;
                this.ProtoStaticObjectToBuild = protoStaticObjectToBuild;
                this.TileOffset = tileOffset;
                this.StartTilePosition = startTilePosition;
                this.ObjectToRelocate = objectToRelocate;
            }
        }

        [NotPersistent]
        public class Validator
        {
            public readonly DelegateCheck Function;

            private readonly bool cacheTheErrorMessageFuncResult;

            private readonly Func<string> errorMessageFunc;

            private string errorMessage;

            public Validator(string errorMessage, DelegateCheck function)
            {
                this.Function = function;
                this.errorMessage = errorMessage;
                this.errorMessageFunc = null;
            }

            public Validator(
                Func<string> errorMessageFunc,
                DelegateCheck function,
                bool cacheTheErrorMessageFuncResult = true)
            {
                this.Function = function;
                this.errorMessageFunc = errorMessageFunc;
                this.cacheTheErrorMessageFuncResult = cacheTheErrorMessageFuncResult;
            }

            public string ErrorMessage
            {
                get
                {
                    if (this.errorMessage is not null)
                    {
                        return this.errorMessage;
                    }

                    if (!this.cacheTheErrorMessageFuncResult)
                    {
                        return this.errorMessageFunc();
                    }

                    return this.errorMessage = this.errorMessageFunc();
                }
            }

            public bool HasErrorMessage
                => this.errorMessage is not null
                   || this.errorMessageFunc is not null;

            public override string ToString()
            {
                return this.ErrorMessage;
            }
        }
    }
}