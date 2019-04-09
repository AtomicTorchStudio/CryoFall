namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Zones;
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
        public const string ErrorCannotBuildInRestrictedArea = "Cannot build in restricted areas.";

        public const string ErrorCannotBuildOnCliffOrSlope = "Cannot build on a cliff or slope.";

        public const string ErrorCannotBuildOnFarmPlot = "Cannot build on farm plot.";

        public const string ErrorCannotBuildOnFloor = "Cannot build on floor.";

        public const string ErrorCreaturesNearby = "There are creatures nearby.";

        public const string ErrorNoFreeSpace = "No free space.";

        public const string ErrorNotSolidGround = "Not a solid ground.";

        public const string ErrorStandingInCell = "You're standing in the cell!";

        public const string ErrorTooCloseToCliffOrSlope = "Cannot build—too close to a cliff or slope.";

        public const string ErrorTooCloseToWater = "Cannot build—too close to water.";

        public const double RequirementNoNpcsRadius = 5;

        public static readonly IConstructionTileRequirementsReadOnly BasicRequirements;

        public static readonly IConstructionTileRequirementsReadOnly DefaultForStaticObjects;

        private static readonly CollisionGroup DefaultCollisionGroup = CollisionGroup.GetDefault();

        private static readonly IWorldService WorldService = Api.IsServer
                                                                 ? (IWorldService)Api.Server.World
                                                                 : Api.Client.World;

        /// <summary>
        /// Checks if there is no dynamic objects in the tile.
        /// </summary>
        public static Validator ValidatorNoPhysicsBodyDynamic
            = new Validator(ErrorNoFreeSpace,
                            c => !TileHasAnyPhysicsObjectsWhere(c.Tile, t => !t.PhysicsBody.IsStatic));

        /// <summary>
        /// Checks if there is any NPC nearby (circle physics check with radius defined by the constant RequirementNoNpcsRadius).
        /// </summary>
        public static Validator ValidatorNoNpcsAround
            = new Validator(ErrorCreaturesNearby,
                            c =>
                            {
                                var physicsSpace = WorldService.GetPhysicsSpace();
                                using (var tempList = physicsSpace.TestCircle(
                                    position: c.Tile.Position.ToVector2D() + (0.5, 0.5),
                                    radius: RequirementNoNpcsRadius,
                                    collisionGroup: DefaultCollisionGroup,
                                    sendDebugEvent: false))
                                {
                                    foreach (var entry in tempList)
                                    {
                                        if (entry.PhysicsBody.AssociatedWorldObject is ICharacter character
                                            && character.IsNpc)
                                        {
                                            // found npc nearby
                                            return false;
                                        }
                                    }

                                    return true;
                                }
                            });

        /// <summary>
        /// (Client only) Checks if there is no current player in the cell.
        /// </summary>
        public static Validator ValidatorClientOnlyNoCurrentPlayer
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
        public static Validator ValidatorNoPhysicsBodyStatic
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

        public static Validator ValidatorNoFarmPlot
            = new Validator(ErrorCannotBuildOnFarmPlot,
                            c => !c.Tile.StaticObjects.Any(
                                     o => o.ProtoStaticWorldObject is IProtoObjectFarmPlot));

        public static Validator ValidatorNoFloor
            = new Validator(ErrorCannotBuildOnFloor,
                            c => !c.Tile.StaticObjects.Any(
                                     o => o.ProtoStaticWorldObject.Kind == StaticObjectKind.Floor));

        // please note - this is server-side validator only, it's safely skipped by client
        public static Validator ValidatorNotRestrictedArea
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

        // please note - this is server-side validator only, it's safely skipped by client
        // this version will prevent even the server from spawning objects there
        public static Validator ValidatorNotRestrictedAreaEvenForServer
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

        private static readonly Lazy<IServerZone> ServerZoneRestrictedArea
            = new Lazy<IServerZone>(
                () => ZoneSpecialConstructionRestricted.Instance.ServerZoneInstance);

        private readonly List<Validator> checkFunctions = new List<Validator>();

        static ConstructionTileRequirements()
        {
            BasicRequirements = new ConstructionTileRequirements()
                                .Add(ErrorNotSolidGround,
                                     c => c.Tile.ProtoTile.Kind == TileKind.Solid)
                                .Add(ErrorCannotBuildOnCliffOrSlope,
                                     c => !c.Tile.IsCliffOrSlope)
                                .Add(ErrorTooCloseToCliffOrSlope,
                                     c => c.Tile.EightNeighborTiles.All(neighbor => !neighbor.IsCliffOrSlope))
                                .Add(ErrorTooCloseToWater,
                                     c => c.Tile.EightNeighborTiles.All(
                                         neighbor => neighbor.ProtoTile.Kind != TileKind.Water));

            DefaultForStaticObjects = BasicRequirements
                                      .Clone()
                                      .Add(ValidatorNoFarmPlot)
                                      .Add(ValidatorNoStaticObjectsExceptFloor)
                                      // ensure no physical objects (static/dynamic) at tile
                                      .Add(ValidatorNoPhysicsBodyStatic)
                                      .AddClientOnly(ValidatorClientOnlyNoCurrentPlayer)
                                      .Add(ValidatorNoPhysicsBodyDynamic);
        }

        public ConstructionTileRequirements(ConstructionTileRequirements toClone)
        {
            this.Add(toClone);
        }

        public ConstructionTileRequirements()
        {
        }

        public delegate bool DelegateCheck(Context context);

        public static bool TileHasAnyPhysicsObjectsWhere(Tile tile, Func<TestResult, bool> check)
        {
            var physicsSpace = WorldService.GetPhysicsSpace();
            using (var tempList = physicsSpace.TestRectangle(
                // include some padding, otherwise the check will include border-objects
                position: tile.Position.ToVector2D() + (0.01, 0.01),
                size: (0.98, 0.98),
                collisionGroup: DefaultCollisionGroup,
                sendDebugEvent: false))
            {
                foreach (var entry in tempList)
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
                                                startTilePosition.Y + tileOffset.Y);
                var context = new Context(tile, character, proto, tileOffset);
                if (this.Check(context, out var errorMessage))
                {
                    continue;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Check(Context context, out string errorMessage)
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

        public struct Validator
        {
            public readonly string ErrorMessage;

            public readonly DelegateCheck Function;

            public Validator(string errorMessage, DelegateCheck function)
            {
                this.Function = function;
                this.ErrorMessage = errorMessage;
            }

            public override string ToString()
            {
                return this.ErrorMessage;
            }
        }
    }
}