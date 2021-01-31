namespace AtomicTorch.CBND.CoreMod.Systems.Construction
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConstructionRelocationSystem : ProtoSystem<ConstructionRelocationSystem>
    {
        public const double MaxRelocationDistance = 15;

        public const int ToolDurabilityCostForStructureRelocation = 10;

        private static ClientComponentObjectPlacementHelper componentObjectPlacementHelper;

        private static ClientComponentObjectRelocationHelper componentRelocationHelper;

        public delegate void DelegateServerStructureRelocation(
            ICharacter character,
            Vector2Ushort fromPosition,
            IStaticWorldObject structure);

        public static event DelegateServerStructureRelocation ServerStructureBeforeRelocating;

        public static event DelegateServerStructureRelocation ServerStructureRelocated;

        public static bool IsInObjectPlacementMode => componentObjectPlacementHelper?.IsEnabled ?? false;

        [NotLocalizable]
        public override string Name => "Construction relocation system";

        public static void ClientDisableConstructionRelocation()
        {
            componentObjectPlacementHelper?.SceneObject.Destroy();
            componentObjectPlacementHelper = null;
            componentRelocationHelper = null;
        }

        public static void ClientStartRelocation(IStaticWorldObject objectStructure)
        {
            var protoStructure = objectStructure.ProtoStaticWorldObject;
            var character = Client.Characters.CurrentPlayerCharacter;

            if (IsInObjectPlacementMode
                || ConstructionPlacementSystem.IsInObjectPlacementMode)
            {
                // already relocating/placing something
                return;
            }

            if (!SharedIsRelocatable(objectStructure))
            {
                return;
            }

            if (!CreativeModeSystem.SharedIsInCreativeMode(character)
                && !LandClaimSystem.SharedIsOwnedLand(objectStructure.TilePosition,
                                                      character,
                                                      requireFactionPermission: true,
                                                      out var hasNoFactionPermission,
                                                      out _))
            {
                // the building location or destination is in an area that is not owned by the player
                SharedShowCannotRelocateNotification(
                    character,
                    protoStructure,
                    hasNoFactionPermission);
                return;
            }

            var isPvEorWithinInteractionArea = PveSystem.SharedIsPve(false)
                                               || protoStructure.SharedIsInsideCharacterInteractionArea(
                                                   Api.Client.Characters.CurrentPlayerCharacter,
                                                   objectStructure,
                                                   writeToLog: false);

            if (!isPvEorWithinInteractionArea)
            {
                CannotInteractMessageDisplay.ClientOnCannotInteract(
                    character,
                    CoreStrings.Notification_TooFar,
                    isOutOfRange: true);
                return;
            }

            if (LandClaimSystem.SharedIsUnderRaidBlock(character, objectStructure))
            {
                // the building is in an area under the raid
                ConstructionSystem.SharedShowCannotBuildNotification(
                    character,
                    LandClaimSystem.ErrorRaidBlockActionRestricted_Message,
                    protoStructure);
                return;
            }

            if (LandClaimShieldProtectionSystem.SharedIsUnderShieldProtection(objectStructure))
            {
                // the building is in an area under shield protection
                LandClaimShieldProtectionSystem.SharedSendNotificationActionForbiddenUnderShieldProtection(
                    character);
                return;
            }

            ClientDisableConstructionRelocation();

            var sceneObject = Client.Scene.CreateSceneObject("StructureRelocationHelper");
            componentObjectPlacementHelper = sceneObject.AddComponent<ClientComponentObjectPlacementHelper>();
            componentRelocationHelper = sceneObject.AddComponent<ClientComponentObjectRelocationHelper>();

            componentObjectPlacementHelper
                .Setup(protoStructure,
                       isCancelable: true,
                       isRepeatCallbackIfHeld: false,
                       isDrawConstructionGrid: true,
                       isBlockingInput: true,
                       validateCanPlaceCallback: ClientValidateCanRelocate,
                       placeSelectedCallback: ClientConstructionPlaceSelectedCallback,
                       delayRemainsSeconds: 0.1);
            componentObjectPlacementHelper.HideBlueprintOnOverlapWithTheSameObject = false;

            componentRelocationHelper.Setup(objectStructure);

            void ClientValidateCanRelocate(
                Vector2Ushort tilePosition,
                bool logErrors,
                out bool canPlace,
                out bool isTooFar)
            {
                if (tilePosition == objectStructure.TilePosition)
                {
                    canPlace = true;
                    isTooFar = false;
                    return;
                }

                if (!SharedCheckTileRequirementsForRelocation(character,
                                                              objectStructure,
                                                              tilePosition,
                                                              logErrors: logErrors))
                {
                    // time requirements are not valid
                    canPlace = false;
                    isTooFar = false;
                    return;
                }

                if (!SharedValidateCanCharacterRelocateStructure(character,
                                                                 objectStructure,
                                                                 tilePosition,
                                                                 logErrors: logErrors))
                {
                    canPlace = true;
                    isTooFar = true;
                    return;
                }

                if (SharedHasObstacle(
                    character,
                    objectStructure,
                    tilePosition.ToVector2D() + protoStructure.Layout.Center))
                {
                    if (logErrors)
                    {
                        CannotInteractMessageDisplay.ClientOnCannotInteract(
                            character,
                            CoreStrings.Notification_ObstaclesOnTheWay,
                            isOutOfRange: true);
                    }

                    canPlace = true;
                    isTooFar = true;
                    return;
                }

                canPlace = true;
                isTooFar = false;
            }

            void ClientConstructionPlaceSelectedCallback(Vector2Ushort tilePosition)
            {
                if (SharedHasObstacle(
                    character,
                    objectStructure,
                    tilePosition.ToVector2D() + protoStructure.Layout.Center))
                {
                    CannotInteractMessageDisplay.ClientOnCannotInteract(
                        character,
                        CoreStrings.Notification_ObstaclesOnTheWay,
                        isOutOfRange: true);
                    return;
                }

                ClientTimersSystem.AddAction(0.1, ClientDisableConstructionRelocation);
                if (tilePosition != objectStructure.TilePosition)
                {
                    Instance.CallServer(_ => _.ServerRemote_RelocateStructure(objectStructure, tilePosition));
                }
            }
        }

        public static bool SharedIsRelocatable(IStaticWorldObject objectStructure)
        {
            var protoStructure = objectStructure.ProtoGameObject as IProtoObjectStructure;
            if (protoStructure is null)
            {
                return false;
            }

            return protoStructure.IsRelocatable
                   && objectStructure.GetPublicState<StaticObjectPublicState>()
                                     .StructurePointsCurrent
                   >= protoStructure.StructurePointsMax;
        }

        public static bool SharedValidateCanCharacterRelocateStructure(
            ICharacter character,
            IStaticWorldObject objectStructure,
            Vector2Ushort toPosition,
            bool logErrors)
        {
            if (!SharedIsRelocatable(objectStructure))
            {
                return false;
            }

            if (!SharedCheckTileRequirementsForRelocation(character,
                                                          objectStructure,
                                                          toPosition,
                                                          logErrors))
            {
                return false;
            }

            if (!(objectStructure.ProtoGameObject is IProtoObjectStructure protoStructure))
            {
                return false;
            }

            if (objectStructure.TilePosition.TileSqrDistanceTo(toPosition)
                > MaxRelocationDistance * MaxRelocationDistance)
            {
                if (logErrors)
                {
                    ConstructionSystem.SharedShowCannotPlaceNotification(
                        character,
                        CoreStrings.Notification_TooFar,
                        protoStructure);
                }

                return false;
            }

            var itemInHands = character.SharedGetPlayerSelectedHotbarItem();
            if (!(itemInHands.ProtoGameObject is IProtoItemToolToolbox))
            {
                return false;
            }

            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            if (!LandClaimSystem.SharedIsOwnedLand(objectStructure.TilePosition,
                                                   character,
                                                   requireFactionPermission: true,
                                                   out var hasNoFactionPermission,
                                                   ownedArea: out _)
                || !IsOwnedLand(toPosition, out hasNoFactionPermission))
            {
                // the building location or destination is in an area that is not owned by the player
                if (logErrors)
                {
                    SharedShowCannotRelocateNotification(
                        character,
                        protoStructure,
                        hasNoFactionPermission);
                }

                return false;
            }

            if (LandClaimSystem.SharedIsUnderRaidBlock(character, objectStructure))
            {
                // the building is in an area under the raid
                if (logErrors)
                {
                    ConstructionSystem.SharedShowCannotPlaceNotification(
                        character,
                        LandClaimSystem.ErrorRaidBlockActionRestricted_Message,
                        protoStructure);
                }

                return false;
            }

            if (LandClaimShieldProtectionSystem.SharedIsUnderShieldProtection(objectStructure))
            {
                // the building is in an area under shield protection
                if (logErrors)
                {
                    LandClaimShieldProtectionSystem.SharedSendNotificationActionForbiddenUnderShieldProtection(
                        character);
                }

                return false;
            }

            return true;

            bool IsOwnedLand(
                Vector2Ushort startTilePosition,
                out bool hasNoFactionPermission)
            {
                var worldObjectLayoutTileOffsets = objectStructure.ProtoStaticWorldObject.Layout.TileOffsets;
                foreach (var tileOffset in worldObjectLayoutTileOffsets)
                {
                    if (!LandClaimSystem.SharedIsOwnedLand(startTilePosition.AddAndClamp(tileOffset),
                                                           character,
                                                           requireFactionPermission: true,
                                                           out hasNoFactionPermission,
                                                           out _))
                    {
                        return false;
                    }
                }

                hasNoFactionPermission = false;
                return true;
            }
        }

        private static bool SharedCheckTileRequirementsForRelocation(
            ICharacter character,
            IStaticWorldObject objectStructure,
            Vector2Ushort toPosition,
            bool logErrors)
        {
            if (!(objectStructure.ProtoGameObject is IProtoObjectStructure protoStructure))
            {
                return false;
            }

            var world = Api.IsServer
                            ? (IWorldService)Server.World
                            : (IWorldService)Client.World;

            var tileRequirements = protoStructure.TileRequirements;
            var occupiedTilePositions = new HashSet<Vector2Ushort>(objectStructure.OccupiedTilePositions);

            foreach (var tileOffset in protoStructure.Layout.TileOffsets)
            {
                var tilePosition = new Vector2Ushort((ushort)(toPosition.X + tileOffset.X),
                                                     (ushort)(toPosition.Y + tileOffset.Y));
                if (occupiedTilePositions.Contains(tilePosition))
                {
                    // no need to check that tile as the object player want to move is already there
                    continue;
                }

                var tile = world.GetTile(tilePosition,
                                         logOutOfBounds: false);

                string errorMessage;

                if (tile.IsOutOfBounds)
                {
                    errorMessage = "Out of bounds";
                }
                else
                {
                    var context = new ConstructionTileRequirements.Context(tile,
                                                                           character,
                                                                           protoStructure,
                                                                           tileOffset,
                                                                           startTilePosition: toPosition,
                                                                           objectToRelocate: objectStructure);
                    if (tileRequirements.Check(context, out errorMessage))
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
                        $"Cannot move {protoStructure} at {toPosition} - check failed:"
                        + Environment.NewLine
                        + errorMessage);
                }

                ConstructionSystem.SharedShowCannotPlaceNotification(
                    character,
                    errorMessage,
                    protoStructure);

                return false;
            }

            return true;
        }

        private static bool SharedHasObstacle(
            ICharacter character,
            IStaticWorldObject forStructureRelocation,
            Vector2D position)
        {
            if (PveSystem.SharedIsPve(false))
            {
                return false;
            }

            var characterCenter = character.Position + character.PhysicsBody.CenterOffset;
            return TestHasObstacle(position);

            // local method for testing if there is an obstacle from current to the specified position
            bool TestHasObstacle(Vector2D toPosition)
            {
                using var obstaclesInTheWay = character.PhysicsBody.PhysicsSpace.TestLine(
                    characterCenter,
                    toPosition,
                    CollisionGroup.Default,
                    sendDebugEvent: false);
                foreach (var test in obstaclesInTheWay.AsList())
                {
                    var testPhysicsBody = test.PhysicsBody;
                    if (testPhysicsBody.AssociatedProtoTile is not null)
                    {
                        // obstacle tile on the way
                        return true;
                    }

                    var testWorldObject = testPhysicsBody.AssociatedWorldObject;
                    if (ReferenceEquals(testWorldObject,    character)
                        || ReferenceEquals(testWorldObject, forStructureRelocation))
                    {
                        // not an obstacle - it's the character or world object itself
                        continue;
                    }

                    switch (testWorldObject.ProtoWorldObject)
                    {
                        case IProtoObjectDeposit: // allow deposits
                        case ObjectWallDestroyed: // allow destroyed walls
                            continue;
                    }

                    // obstacle object on the way
                    return true;
                }

                // no obstacles
                return false;
            }
        }

        private static void SharedShowCannotRelocateNotification(
            ICharacter character,
            IProtoStaticWorldObject protoStructure,
            bool hasNoFactionPermission)
        {
            if (IsServer)
            {
                Instance.CallClient(character,
                                    _ => _.ClientRemote_ShowCannotRelocateNotification(
                                        protoStructure,
                                        hasNoFactionPermission));
                return;
            }

            if (hasNoFactionPermission)
            {
                NotificationSystem.ClientShowNotification(
                    title: CoreStrings.Notification_ActionForbidden,
                    string.Format(CoreStrings.Faction_Permission_Required_Format,
                                  CoreStrings.Faction_Permission_LandClaimManagement_Title),
                    NotificationColor.Bad,
                    protoStructure.Icon);
            }
            else
            {
                NotificationSystem.ClientShowNotification(
                    title: CoreStrings.Notification_ActionForbidden,
                    LandClaimSystem.ErrorNotLandOwner_Message,
                    NotificationColor.Bad,
                    protoStructure.Icon);
            }

            return;
        }

        private void ClientRemote_ShowCannotRelocateNotification(
            IProtoStaticWorldObject protoStructure,
            bool hasNoFactionPermission)
        {
            SharedShowCannotRelocateNotification(ClientCurrentCharacterHelper.Character,
                                                 protoStructure,
                                                 hasNoFactionPermission);
        }

        private void ServerRemote_RelocateStructure(IStaticWorldObject objectStructure, Vector2Ushort toPosition)
        {
            if (objectStructure.TilePosition == toPosition)
            {
                // relocation not required
                return;
            }

            var character = ServerRemoteContext.Character;
            if (!SharedValidateCanCharacterRelocateStructure(character,
                                                             objectStructure,
                                                             toPosition,
                                                             logErrors: true))
            {
                return;
            }

            var fromPosition = objectStructure.TilePosition;

            Api.SafeInvoke(
                () => ServerStructureBeforeRelocating?.Invoke(character, fromPosition, objectStructure));

            Server.World.SetPosition(objectStructure, toPosition);

            try
            {
                // ensure the structure is reinitialized (has its physics rebuilt, etc)
                objectStructure.ServerInitialize();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

            ConstructionPlacementSystem.Instance.ServerNotifyOnStructurePlacedOrRelocated(objectStructure, character);

            Api.SafeInvoke(
                () => ServerStructureRelocated?.Invoke(character, fromPosition, objectStructure));

            // let's deduct the tool durability
            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return;
            }

            // the item in hotbar is definitely a construction tool as it was validated above
            var itemConstructionTool = character.SharedGetPlayerSelectedHotbarItem();
            ItemDurabilitySystem.ServerModifyDurability(itemConstructionTool,
                                                        delta: -ToolDurabilityCostForStructureRelocation);
        }
    }
}