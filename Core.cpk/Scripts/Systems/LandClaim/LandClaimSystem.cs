namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Perks;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Deconstruction;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.RaidingProtection;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.StructureDecaySystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class LandClaimSystem : ProtoSystem<LandClaimSystem>
    {
        public const string ErrorCannotBuild_AnotherLandClaimTooClose =
            "Too close to another land claim.";

        public const string ErrorCannotBuild_AreaIsClaimedOrTooCloseToClaimed =
            "The area is claimed by another player, or it's too close to someone else's land claim.";

        // The game has a resource contesting system preventing the players
        // to claim or build near the recently spawned oil and other resource deposits.
        public const string ErrorCannotBuild_DepositCooldown =
            "The resource deposit has only just appeared and cannot be claimed yet. Construction around it is temporary restricted.";

        public const string ErrorCannotBuild_IntersectingWithAnotherLandClaim =
            "Intersecting with another land claim area";

        public const string ErrorCannotBuild_LandClaimAmountLimitExceeded =
            @"You've used up your allotted number of land claims.
              [br]You can increase that number by researching new technologies.";

        public const string ErrorCannotBuild_NeedXenogeologyTech =
            "You need to research Xenogeology (Tier 3) to claim the Oil/Li deposits or to place a land claim close to it.";

        public const string ErrorCannotBuild_RaidUnderWay =
            @"Raid under way.
              [br]You cannot build new structures while your base is under attack.";

        public const string ErrorCannotBuild_RequiresOwnedArea =
            "You can place this structure only inside your land claim area.";

        public const string ErrorNotLandOwner_Message =
            "You're not the land area owner.";

        public const string ErrorRaidBlockActionRestricted_Message =
            "The base is under raid.";

        /// <summary>
        /// Determines how many protected cells should be around the max land claim area to prevent
        /// building in this neutral (grace) area by other players or claiming it.
        /// </summary>
        public const byte MinPaddingSizeOneDirection = 1;

        /// <summary>
        /// Determines the duration of "raid block" feature - preventing players from
        /// repairing and building new structures after the bomb is exploded within their land claim area.
        /// Applies only to bombs (except mining charge).
        /// </summary>
        public const double RaidBlockDurationSeconds = 10 * 60; // 10 minutes

        private static ILogicObject serverLandClaimManagerInstance;

        private static IList<ILogicObject> sharedLandClaimAreas
            = IsClient
                  ? new List<ILogicObject>() // workaround for client so it will be able to query (empty) list
                  : null;

        public static readonly ConstructionTileRequirements.Validator ValidatorIsOwnedOrFreeArea
            = new ConstructionTileRequirements.Validator(
                ErrorCannotBuild_AreaIsClaimedOrTooCloseToClaimed,
                context =>
                {
                    var forCharacter = context.CharacterBuilder;
                    if (forCharacter == null)
                    {
                        return true;
                    }

                    if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                    {
                        return true;
                    }

                    var position = context.Tile.Position;

                    var noAreasThere = true;
                    foreach (var area in sharedLandClaimAreas)
                    {
                        var areaBoundsDirect = SharedGetLandClaimAreaBounds(area);
                        var areaBoundsWithPadding = areaBoundsDirect.Inflate(LandClaimArea.GetPublicState(area)
                                                                                          .LandClaimGraceAreaPaddingSizeOneDirection);

                        if (!areaBoundsWithPadding.Contains(position))
                        {
                            // there is no area (even with the padding/grace area)
                            continue;
                        }

                        if (areaBoundsDirect.Contains(position))
                        {
                            // there is a direct area (not padding/grace area)
                            if (SharedIsOwnedArea(area, forCharacter))
                            {
                                // player owns the area
                                return true;
                            }

                            noAreasThere = false;
                        }
                        else
                        {
                            // there is no direct area (only the padding/grace area)
                            if (!SharedIsOwnedArea(area, forCharacter))
                            {
                                // the grace/padding area of another player's land claim found
                                noAreasThere = false;
                            }
                        }
                    }

                    // if we've come to here here and there are any areas it means the player doesn't own any of them
                    return noAreasThere;
                });

        public static readonly ConstructionTileRequirements.Validator ValidatorIsOwnedArea
            = new ConstructionTileRequirements.Validator(
                ErrorCannotBuild_RequiresOwnedArea,
                context =>
                {
                    var forCharacter = context.CharacterBuilder;
                    if (forCharacter == null)
                    {
                        return true;
                    }

                    if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                    {
                        return true;
                    }

                    return SharedIsOwnedLand(context.Tile.Position,
                                             forCharacter,
                                             out _);
                });

        public static readonly ConstructionTileRequirements.Validator ValidatorNoRaid
            = new ConstructionTileRequirements.Validator(
                ErrorCannotBuild_RaidUnderWay,
                context =>
                {
                    var forCharacter = context.CharacterBuilder;
                    if (forCharacter == null)
                    {
                        return true;
                    }

                    if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                    {
                        return true;
                    }

                    var position = context.Tile.Position;
                    foreach (var area in sharedLandClaimAreas)
                    {
                        var areaBounds = SharedGetLandClaimAreaBounds(area, addGracePadding: true);
                        if (!areaBounds.Contains(position))
                        {
                            continue;
                        }

                        // only server and client owning the area has the private state of the area
                        // to check whether it's under raid or not
                        if (SharedIsAreaUnderRaid(area))
                        {
                            // cannot build - there is an area under raid   
                            return false;
                        }
                    }

                    return true;
                });

        public static readonly ConstructionTileRequirements.Validator ValidatorNewLandClaimNoLandClaimIntersections
            = new ConstructionTileRequirements.Validator(
                ErrorCannotBuild_IntersectingWithAnotherLandClaim,
                context =>
                {
                    var forCharacter = context.CharacterBuilder;
                    if (forCharacter == null)
                    {
                        return true;
                    }

                    if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                    {
                        return true;
                    }

                    var protoObjectLandClaim = (IProtoObjectLandClaim)context.ProtoStaticObjectToBuild;
                    if (context.TileOffset
                        != SharedCalculateLandClaimObjectCenterTilePosition(Vector2Ushort.Zero, protoObjectLandClaim))
                    {
                        // we don't check offset tiles
                        // as the land claim area calculated from the center tile of the land claim object
                        return true;
                    }

                    var centerTilePosition = context.Tile.Position;
                    return SharedCheckCanPlaceOrUpgradeLandClaimThere(protoObjectLandClaim,
                                                                      centerTilePosition,
                                                                      forCharacter);
                });

        public static readonly ConstructionTileRequirements.Validator ValidatorNewLandClaimNoLandClaimsNearby
            = new ConstructionTileRequirements.Validator(
                ErrorCannotBuild_AnotherLandClaimTooClose,
                context =>
                {
                    var forCharacter = context.CharacterBuilder;
                    if (forCharacter == null)
                    {
                        return true;
                    }

                    if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                    {
                        return true;
                    }

                    var protoObjectLandClaim = (IProtoObjectLandClaim)context.ProtoStaticObjectToBuild;
                    if (context.TileOffset
                        != SharedCalculateLandClaimObjectCenterTilePosition(Vector2Ushort.Zero, protoObjectLandClaim))
                    {
                        // we don't check offset tiles
                        // as the land claim area calculated from the center tile of the land claim object
                        return true;
                    }

                    var centerTilePosition = context.Tile.Position;
                    var testBounds = SharedCalculateLandClaimAreaBounds(
                        centerTilePosition,
                        // use special test size - it will result in 5 tiles min padding around the built land claims
                        size: 13);

                    foreach (var area in sharedLandClaimAreas)
                    {
                        var areaCenterLocation = LandClaimArea.GetPublicState(area)
                                                              .LandClaimCenterTilePosition;
                        if (testBounds.Contains(areaCenterLocation))
                        {
                            // too close, check failed
                            return false;
                        }
                    }

                    return true;
                });

        public static readonly ConstructionTileRequirements.Validator ValidatorCheckCharacterLandClaimAmountLimit
            = new ConstructionTileRequirements.Validator(
                ErrorCannotBuild_LandClaimAmountLimitExceeded,
                context =>
                {
                    var forCharacter = context.CharacterBuilder;
                    if (forCharacter == null)
                    {
                        return true;
                    }

                    if (context.TileOffset != Vector2Int.Zero)
                    {
                        return true;
                    }

                    if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                    {
                        return true;
                    }

                    // calculate max number of land claims
                    var finalStatsCache = forCharacter.SharedGetFinalStatsCache();
                    var maxNumber = (int)Math.Round(finalStatsCache[StatName.LandClaimsMaxNumber],
                                                    MidpointRounding.AwayFromZero);

                    // calculate current number of land claims
                    var currentNumber = 0;
                    foreach (var area in sharedLandClaimAreas)
                    {
                        if (IsClient && !area.ClientHasPrivateState)
                        {
                            continue;
                        }

                        var privateState = LandClaimArea.GetPrivateState(area);
                        if (privateState.LandClaimFounder == forCharacter.Name)
                        {
                            currentNumber++;
                        }
                    }

                    return maxNumber > currentNumber; // ok only in case when the limit is not exceeded
                });

        public static readonly ConstructionTileRequirements.Validator
            ValidatorCheckCharacterLandClaimDepositRequireXenogeology
                = new ConstructionTileRequirements.Validator(
                    ErrorCannotBuild_NeedXenogeologyTech,
                    context =>
                    {
                        var forCharacter = context.CharacterBuilder;
                        if (forCharacter == null)
                        {
                            return true;
                        }

                        if (context.TileOffset != Vector2Int.Zero)
                        {
                            return true;
                        }

                        if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                        {
                            return true;
                        }

                        if (PerkClaimDeposits.Instance
                                             .SharedIsPerkUnlocked(forCharacter))
                        {
                            // has perk unlocked so no need to check whether player trying to claim a deposit
                            return true;
                        }

                        // doesn't have Xenogeology unlocked
                        // check whether there are any deposits nearby
                        var forbiddenArea = SharedCalculateLandClaimAreaBounds(
                            centerTilePosition: (context.Tile.Position
                                                 + context.ProtoStaticObjectToBuild.Layout.Center.ToVector2Int())
                            .ToVector2Ushort(),
                            // We use a carefully selected size here to ensure the deposit
                            // cannot surrounded by land claims of players without Xenogeology.
                            // Some extra padding is also useful here though not really necessary.
                            size: (ushort)(6 + 2 * MaxLandClaimSizeWithGraceArea.Value));

                        foreach (var mark in WorldMapResourceMarksSystem.SharedEnumerateMarks())
                        {
                            var staticWorldObjectBounds = new BoundsInt(
                                mark.Position,
                                // TODO: it's a hardcoded oil/Li deposit size
                                new Vector2Int(3, 3));

                            if (forbiddenArea.Intersects(staticWorldObjectBounds))
                            {
                                // found a deposit in bounds of the future land claim
                                return false;
                            }
                        }

                        return true;
                    });

        public static readonly Lazy<ushort> MaxLandClaimSize
            = new Lazy<ushort>(() => Api.FindProtoEntities<IProtoObjectLandClaim>()
                                        .Max(l => l.LandClaimSize));

        public static readonly Lazy<ushort> MaxLandClaimSizeWithGraceArea
            = new Lazy<ushort>(() => (ushort)(MaxLandClaimSize.Value + MinPaddingSizeOneDirection * 2));

        public static readonly ConstructionTileRequirements.Validator ValidatorCheckCharacterLandClaimDepositCooldown
            = new ConstructionTileRequirements.Validator(
                ErrorCannotBuild_DepositCooldown,
                context =>
                {
                    var forCharacter = context.CharacterBuilder;
                    if (forCharacter == null)
                    {
                        return true;
                    }

                    if (context.TileOffset != Vector2Int.Zero)
                    {
                        return true;
                    }

                    if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                    {
                        return true;
                    }

                    var restrictionSize = DepositResourceLandClaimRestrictionSize.Value;
                    foreach (var mark in WorldMapResourceMarksSystem.SharedEnumerateMarks())
                    {
                        var timeRemainsToClaimCooldown =
                            (int)WorldMapResourceMarksSystem.SharedCalculateTimeToClaimLimitRemovalSeconds(
                                mark.ServerSpawnTime);
                        if (timeRemainsToClaimCooldown <= 0)
                        {
                            continue;
                        }

                        var bounds = new BoundsInt(
                            mark.Position - (restrictionSize.X / 2, restrictionSize.Y / 2),
                            restrictionSize);

                        if (bounds.Contains(context.Tile.Position))
                        {
                            // cannot claim this deposit yet
                            return false;
                        }
                    }

                    return true;
                });

        protected internal static readonly IWorldClientService ClientWorld
            = IsClient ? Client.World : null;

        private static readonly Lazy<Vector2Ushort> DepositResourceLandClaimRestrictionSize
            = new Lazy<Vector2Ushort>(
                () => new Vector2Ushort((ushort)(8 + 2 * MaxLandClaimSizeWithGraceArea.Value),
                                        (ushort)(8 + 2 * MaxLandClaimSizeWithGraceArea.Value)));

        // how long the items dropped on the ground from the safe storage should remain there
        private static readonly TimeSpan DestroyedLandClaimDroppedItemsDestructionTimeout = TimeSpan.FromDays(1);

        private static readonly ICharactersServerService ServerCharacters
            = IsServer ? Server.Characters : null;

        private static readonly IWorldServerService ServerWorld
            = IsServer ? Server.World : null;

        public delegate void DelegateServerObjectLandClaimDestroyed(
            IStaticWorldObject landClaimStructure,
            RectangleInt areaBounds);

        public delegate void ServerLandClaimsGroupChangedDelegate(
            ILogicObject area,
            [CanBeNull] ILogicObject areasGroupFrom,
            [CanBeNull] ILogicObject areasGroupTo);

        public static event ServerLandClaimsGroupChangedDelegate ServerAreaGroupChanged;

        public static event Action<ILogicObject> ServerAreasGroupCreated;

        public static event Action<ILogicObject> ServerAreasGroupDestroyed;

        public static event DelegateServerObjectLandClaimDestroyed ServerObjectLandClaimDestroyed;

        public override string Name => "Land claim system";

        public static Task<LandClaimsGroupDecayInfo> ClientGetDecayInfoText(IStaticWorldObject landClaimWorldObject)
        {
            return Instance.CallServer(_ => _.ServerRemote_GetDecayInfo(landClaimWorldObject));
        }

        public static bool ClientIsOwnedArea(ILogicObject area)
        {
            if (area == null)
            {
                throw new ArgumentNullException(nameof(area));
            }

            if (!(area.ProtoLogicObject is LandClaimArea))
            {
                throw new Exception($"{area} is not a {nameof(LandClaimArea)}");
            }

            return area.ClientHasPrivateState;
        }

        public static async void ClientSetAreaOwners(ILogicObject area, List<string> newOwners)
        {
            var errorMessage = await Instance.CallServer(
                                   _ => Instance.ServerRemote_SetAreaOwners(area, newOwners));
            if (errorMessage == null)
            {
                return;
            }

            NotificationSystem.ClientShowNotification(
                title: null,
                message: errorMessage,
                color: NotificationColor.Bad);
        }

        public static void ClientShowNotificationActionForbiddenUnderRaidblock()
        {
            NotificationSystem.ClientShowNotification(
                CoreStrings.Notification_ActionForbidden,
                ErrorRaidBlockActionRestricted_Message,
                color: NotificationColor.Bad);
        }

        public static double ServerAdjustDamageToUnprotectedStrongBuilding(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damageMultiplier)
        {
            var prototype = targetObject.ProtoStaticWorldObject;
            if (prototype.StructurePointsMax < 1000)
            {
                // considered not a strong building
                return damageMultiplier;
            }

            var layoutBounds = prototype.Layout.Bounds;
            if (SharedIsLandClaimedByAnyone(
                new RectangleInt(offset: targetObject.TilePosition + layoutBounds.Offset,
                                 size: layoutBounds.Size)))
            {
                // protected building
                return damageMultiplier;
            }

            // unprotected building
            damageMultiplier *= weaponCache.ProtoObjectExplosive == null
                                    ? 50 // damage multiplier for weapon
                                    : 8; // damage multiplier for explosive

            return damageMultiplier;
        }

        public static ILogicObject ServerFindLandClaimAreasGroup(
            Vector2Ushort tilePosition,
            bool addGracePadding = false)
        {
            // TODO: the lookup is really slow on the populated servers. Consider Grid or QuadTree optimization to locate areas quickly.
            foreach (var area in sharedLandClaimAreas)
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area, addGracePadding);
                if (!areaBounds.Contains(tilePosition))
                {
                    continue;
                }

                var areaPublicState = LandClaimArea.GetPublicState(area);
                var areasGroup = areaPublicState.LandClaimAreasGroup;
                return areasGroup;
            }

            return null;
        }

        public static double ServerGetDecayDelayDurationForLandClaimAreas(List<ILogicObject> areas)
        {
            var decayDelayDuration = StructureConstants.StructuresAbandonedDecayDelaySeconds;
            foreach (var area in areas)
            {
                var worldObject = LandClaimArea.GetPrivateState(area)
                                               .ServerLandClaimWorldObject;

                var protoObjectLandClaim = (IProtoObjectLandClaim)worldObject.ProtoStaticWorldObject;
                decayDelayDuration = Math.Max(protoObjectLandClaim.DecayDelayDuration.TotalSeconds
                                              * StructureConstants.StructuresLandClaimDecayDelayDurationMultiplier,
                                              decayDelayDuration);
            }

            return decayDelayDuration;
        }

        public static ILogicObject ServerGetLandClaimArea(IStaticWorldObject landClaimStructure)
        {
            return landClaimStructure.GetPublicState<ObjectLandClaimPublicState>()
                                     .LandClaimAreaObject;
        }

        public static bool ServerIsOwnedArea(ILogicObject area, ICharacter character)
        {
            if (area == null)
            {
                Logger.Warning(nameof(ServerIsOwnedArea) + " - argument area is null");
                return false;
            }

            var privateState = LandClaimArea.GetPrivateState(area);
            return privateState.LandOwners
                               .Contains(character.Name);
        }

        public static void ServerNotifyCannotInteractNotOwner(ICharacter character, IStaticWorldObject worldObject)
        {
            Instance.CallClient(character, _ => _.ClientRemote_OnCannotInteractNotOwner(worldObject));
        }

        public static void ServerOnObjectLandClaimBuilt(
            ICharacter byCharacter,
            IStaticWorldObject landClaimStructure)
        {
            if (!(landClaimStructure?.ProtoStaticWorldObject
                      is IProtoObjectLandClaim))
            {
                throw new Exception("Not a land claim structure: " + landClaimStructure);
            }

            // create new area for this land claim structure
            var area = Api.Server.World.CreateLogicObject<LandClaimArea>();
            Logger.Important($"Created land claim area: {area}");
            var areaPrivateState = LandClaimArea.GetPrivateState(area);
            var areaPublicState = LandClaimArea.GetPublicState(area);
            var founderName = byCharacter.Name;

            // setup it
            areaPrivateState.ServerLandClaimWorldObject = landClaimStructure;
            areaPrivateState.LandClaimFounder = founderName;
            areaPrivateState.LandOwners = new NetworkSyncList<string>()
            {
                founderName
            };

            areaPublicState.SetupAreaProperties(areaPrivateState);

            // set this area to the structure public state
            landClaimStructure.GetPublicState<ObjectLandClaimPublicState>()
                              .LandClaimAreaObject = area;

            ServerEstablishAreasGroup(
                SharedCalculateLandClaimAreaBounds(area));

            ServerOnAddLandOwner(area, byCharacter, notify: false);

            Logger.Important("Land claim area added: " + area);
        }

        public static void ServerOnObjectLandClaimDestroyed(
            IStaticWorldObject landClaimStructure)
        {
            if (!(landClaimStructure?.ProtoStaticWorldObject
                      is IProtoObjectLandClaim))
            {
                throw new Exception("Not a land claim structure: " + landClaimStructure);
            }

            var area = ServerGetLandClaimArea(landClaimStructure);
            if (area == null)
            {
                // area was already released (upgrade?)
                return;
            }

            var areaBounds = SharedGetLandClaimAreaBounds(area);

            StructureDecaySystem.ServerBeginDecayForStructuresInArea(areaBounds);
            var areaPublicState = LandClaimArea.GetPublicState(area);
            var areasGroup = areaPublicState.LandClaimAreasGroup;
            var areasGroupPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            var areasGroupBounds = SharedGetLandClaimGroupsBoundingArea(areasGroup);
            using var oldAreasInGroup = Api.Shared.WrapInTempList(areasGroupPrivateState.ServerLandClaimsAreas);
            oldAreasInGroup.Remove(area);

            ServerWorld.DestroyObject(area);
            Logger.Important("Land claim area removed: " + area);

            ServerCleanupAreasGroup(areasGroup);

            if (areasGroup.IsDestroyed)
            {
                // last land claim destroyed
                ServerDropItems(landClaimStructure, areasGroup);
            }

            ServerTryRebuildLandClaimsGroups(areasGroupBounds);

            if (ServerCharacters.EnumerateAllPlayerCharacters(onlyOnline: true)
                                .Any(
                                    c =>
                                        PlayerCharacter.GetPublicState(c).CurrentPublicActionState is
                                            DeconstructionActionState.PublicState deconstructionState
                                        && deconstructionState.TargetWorldObject == landClaimStructure))
            {
                // land claim structure is deconstructed by a crowbar
            }
            else
            {
                // land claim structure simply destroyed
                Api.SafeInvoke(() => ServerObjectLandClaimDestroyed?.Invoke(landClaimStructure, areaBounds));
            }

            if (!oldAreasInGroup.SequenceEqual(areasGroupPrivateState.ServerLandClaimsAreas))
            {
                // areas group changed/broken to several areas
                var newGroups = oldAreasInGroup
                                .GroupBy(areas => LandClaimArea.GetPublicState(areas).LandClaimAreasGroup)
                                .Select(g => g.Key)
                                .ToList();

                newGroups.Remove(areasGroup);
                LandClaimAreasGroup.ServerOnBaseBroken(areasGroup, newGroups);
            }
        }

        public static void ServerOnRaid(RectangleInt bounds, ICharacter byCharacter)
        {
            if (byCharacter == null)
            {
                return;
            }

            if (!RaidingProtectionSystem.SharedCanRaid(bounds, showClientNotification: false))
            {
                // Raiding is not possible now.
                return;
            }

            using var tempList = Api.Shared.GetTempList<ILogicObject>();
            SharedGetAreasInBounds(bounds, tempList, addGracePadding: false);

            foreach (var area in tempList)
            {
                if (ServerIsOwnedArea(area, byCharacter))
                {
                    // don't start the raid timer if attack is performed by the owner of the area
                    continue;
                }

                ServerSetRaidblock(area);
            }
        }

        public static void ServerRegisterArea(ILogicObject area)
        {
            Api.Assert(area.ProtoLogicObject is LandClaimArea, "Wrong object type");
            sharedLandClaimAreas.Add(area);

            var owners = area.GetPrivateState<LandClaimAreaPrivateState>()
                             .LandOwners;
            if (owners == null)
            {
                return;
            }

            foreach (var owner in owners)
            {
                var player = ServerCharacters.GetPlayerCharacter(owner);
                if (player != null)
                {
                    SharedGetPlayerOwnedAreas(player).Add(area);
                }
            }
        }

        public static void ServerSetRaidblock(ILogicObject area)
        {
            var areaPrivateState = LandClaimArea.GetPrivateState(area);
            var areaPublicState = LandClaimArea.GetPublicState(area);
            var areasGroup = areaPublicState.LandClaimAreasGroup;
            var areasGroupPublicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            var time = Server.Game.FrameTime;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (areasGroupPublicState.LastRaidTime == time)
            {
                return;
            }

            areasGroupPublicState.LastRaidTime = time;
            Logger.Important(
                string.Format("Land claim area(s) being raided: {1}{0}Areas group: {2}{0}Land owners: {3}",
                              Environment.NewLine,
                              areaPrivateState.ServerLandClaimWorldObject,
                              areasGroup,
                              areaPrivateState.LandOwners.GetJoinedString()));
        }

        public static void ServerUnregisterArea(ILogicObject area)
        {
            Api.Assert(area.ProtoLogicObject is LandClaimArea, "Wrong object type");
            sharedLandClaimAreas.Remove(area);

            var owners = area.GetPrivateState<LandClaimAreaPrivateState>()
                             .LandOwners;
            if (owners == null)
            {
                return;
            }

            foreach (var owner in owners)
            {
                var player = ServerCharacters.GetPlayerCharacter(owner);
                if (player != null)
                {
                    SharedGetPlayerOwnedAreas(player).Remove(area);
                }
            }
        }

        public static IStaticWorldObject ServerUpgrade(
            IStaticWorldObject oldStructure,
            IProtoObjectStructure upgradeStructure,
            ICharacter character)
        {
            if (!(oldStructure?.ProtoStaticWorldObject
                      is IProtoObjectLandClaim))
            {
                throw new Exception("Not a land claim structure: " + oldStructure);
            }

            var tilePosition = oldStructure.TilePosition;
            var area = ServerGetLandClaimArea(oldStructure);

            // release area
            oldStructure.GetPublicState<ObjectLandClaimPublicState>().LandClaimAreaObject = null;

            // destroy old structure
            ServerWorld.DestroyObject(oldStructure);

            // create new structure
            var upgradedObject = ServerWorld.CreateStaticWorldObject(upgradeStructure, tilePosition);

            // get area for the old land claim structure
            var areaPrivateState = LandClaimArea.GetPrivateState(area);
            var areaPublicState = LandClaimArea.GetPublicState(area);

            // update it to use upgraded land claim structure
            areaPrivateState.ServerLandClaimWorldObject = upgradedObject;
            areaPublicState.SetupAreaProperties(areaPrivateState);

            // set this area to the structure public state
            upgradedObject.GetPublicState<ObjectLandClaimPublicState>()
                          .LandClaimAreaObject = area;

            Logger.Important($"Successfully upgraded: {oldStructure} to {upgradedObject}", character);

            Instance.CallClient(
                ServerCharacters.EnumerateAllPlayerCharacters(onlyOnline: true),
                _ => _.ClientRemote_OnLandClaimUpgraded(area));

            ServerEstablishAreasGroup(
                SharedCalculateLandClaimAreaBounds(area));

            // Even though the area's group didn't change,
            // we need to notify power system so it will rebuild power grid for the area's group.
            Api.SafeInvoke(() => ServerAreaGroupChanged?.Invoke(area,
                                                                areaPublicState.LandClaimAreasGroup,
                                                                null));

            return upgradedObject;
        }

        public static RectangleInt SharedCalculateLandClaimAreaBounds(Vector2Ushort centerTilePosition, ushort size)
        {
            var worldBounds = IsServer
                                  ? ServerWorld.WorldBounds
                                  : ClientWorld.WorldBounds;
            var pos = centerTilePosition;
            var halfSize = size / 2.0;

            var start = new Vector2Ushort(
                (ushort)Math.Max(Math.Ceiling(pos.X - halfSize), worldBounds.MinX),
                (ushort)Math.Max(Math.Ceiling(pos.Y - halfSize), worldBounds.MinY));

            var endX = Math.Min(Math.Ceiling(pos.X + halfSize), worldBounds.MaxX);
            var endY = Math.Min(Math.Ceiling(pos.Y + halfSize), worldBounds.MaxY);

            var calculatedSize = new Vector2Ushort((ushort)(endX - start.X),
                                                   (ushort)(endY - start.Y));
            return new RectangleInt(start, size: calculatedSize);
        }

        public static RectangleInt SharedCalculateLandClaimAreaBounds(ILogicObject area)
        {
            var areaPublicState = LandClaimArea.GetPublicState(area);
            return SharedCalculateLandClaimAreaBounds(areaPublicState.LandClaimCenterTilePosition,
                                                      areaPublicState.LandClaimSize);
        }

        public static ushort SharedCalculateLandClaimGraceAreaPaddingSizeOneDirection(ushort landClaimSize)
        {
            var deltaSize = MaxLandClaimSize.Value - landClaimSize;
            if (deltaSize % 2 != 0)
            {
                throw new Exception("Problem with the land claim size: it should be an even number");
            }

            deltaSize /= 2;
            return (ushort)(deltaSize
                            + MinPaddingSizeOneDirection);
        }

        public static Vector2Ushort SharedCalculateLandClaimObjectCenterTilePosition(
            Vector2Ushort tilePosition,
            IProtoObjectLandClaim protoObjectLandClaim)
        {
            return tilePosition.AddAndClamp(protoObjectLandClaim.Layout.Center.ToVector2Int());
        }

        public static Vector2Ushort SharedCalculateLandClaimObjectCenterTilePosition(IStaticWorldObject landClaimObject)
        {
            return SharedCalculateLandClaimObjectCenterTilePosition(
                landClaimObject.TilePosition,
                (IProtoObjectLandClaim)landClaimObject.ProtoStaticWorldObject);
        }

        public static bool SharedCheckCanDeconstruct(IStaticWorldObject worldObject, ICharacter character)
        {
            // Please note: the game already have validated that the target object is a structure
            if (worldObject.ProtoGameObject is ObjectWallDestroyed)
            {
                // always allow deconstruct a destroyed wall object even if it's in another player's land claim
                return true;
            }

            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                // operator can deconstruct any structure
                return true;
            }

            RectangleInt worldObjectBounds;
            {
                var temp = worldObject.ProtoStaticWorldObject.Layout.Bounds;
                var tilePosition = worldObject.TilePosition;
                worldObjectBounds = new RectangleInt(
                    temp.MinX + tilePosition.X,
                    temp.MinY + tilePosition.Y,
                    temp.Size.X,
                    temp.Size.Y);
            }

            // let's check whether there are any areas and player own them
            var isThereAnyArea = false;
            foreach (var area in sharedLandClaimAreas)
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (!areaBounds.IntersectsLoose(worldObjectBounds))
                {
                    continue;
                }

                isThereAnyArea = true;
                // intersection with area found - check if player owns the area
                if (SharedIsOwnedArea(area, character))
                {
                    // player own the land claim area
                    return true;
                }
            }

            if (isThereAnyArea)
            {
                // found a not owned area
                return false;
            }

            // no area found
            if (worldObject.ProtoGameObject is ProtoObjectConstructionSite)
            {
                // can deconstruct blueprints if there is no land claim area
                return true;
            }

            // let's check whether there is a grace area of the land claim area owned by the player
            foreach (var area in sharedLandClaimAreas)
            {
                var areaBoundsWithPadding = SharedGetLandClaimAreaBounds(area, addGracePadding: true);
                if (!areaBoundsWithPadding.IntersectsLoose(worldObjectBounds))
                {
                    continue;
                }

                // intersection with grace area found - check if player owns the area
                if (SharedIsOwnedArea(area, character))
                {
                    // player own the land claim area containing this grace area
                    return true;
                }
            }

            return false;
        }

        // need to verify that area bounds will not intersect with the any existing areas (except from the same founder)
        public static bool SharedCheckCanPlaceOrUpgradeLandClaimThere(
            IProtoObjectLandClaim newProtoObjectLandClaim,
            Vector2Ushort landClaimCenterTilePosition,
            ICharacter forCharacter)
        {
            if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
            {
                return true;
            }

            var newAreaBounds = SharedCalculateLandClaimAreaBounds(
                landClaimCenterTilePosition,
                newProtoObjectLandClaim.LandClaimWithGraceAreaSize);

            foreach (var area in sharedLandClaimAreas)
            {
                var areaBoundsWithPadding = SharedGetLandClaimAreaBounds(area, addGracePadding: true);
                if (!areaBoundsWithPadding.IntersectsLoose(newAreaBounds))
                {
                    // there is no area (even with the padding/grace area)
                    continue;
                }

                if (!SharedIsOwnedArea(area, forCharacter))
                {
                    // the grace/padding area of another player's land claim owner
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<ILogicObject> SharedEnumerateAllAreas()
        {
            return sharedLandClaimAreas;
        }

        public static void SharedGetAreasInBounds(
            RectangleInt bounds,
            ITempList<ILogicObject> result,
            bool addGracePadding)
        {
            if (result.Count > 0)
            {
                result.Clear();
            }

            // TODO: the lookup is really slow on the populated servers. Consider Grid or QuadTree optimization to locate areas quickly.
            foreach (var area in sharedLandClaimAreas)
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (addGracePadding)
                {
                    areaBounds = areaBounds.Inflate(
                        LandClaimArea.GetPublicState(area)
                                     .LandClaimGraceAreaPaddingSizeOneDirection);
                }

                if (areaBounds.IntersectsLoose(bounds))
                {
                    result.Add(area);
                }
            }
        }

        public static RectangleInt SharedGetLandClaimAreaBounds(
            ILogicObject area,
            bool addGracePadding = false)
        {
            var publicState = LandClaimArea.GetPublicState(area);
            var size = publicState.LandClaimSize;
            if (addGracePadding)
            {
                size = (ushort)(size + publicState.LandClaimGraceAreaPaddingSizeOneDirection * 2);
            }

            return SharedCalculateLandClaimAreaBounds(
                publicState.LandClaimCenterTilePosition,
                size);
        }

        public static ILogicObject SharedGetLandClaimAreaGroup(ILogicObject area)
        {
            return LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
        }

        public static RectangleInt SharedGetLandClaimGroupsBoundingArea(
            ILogicObject group,
            bool addGraceAreaPadding = false)
        {
            var hasBounds = false;
            ushort minX = ushort.MaxValue,
                   minY = ushort.MaxValue,
                   maxX = 0,
                   maxY = 0;
            foreach (var area in sharedLandClaimAreas)
            {
                var areaPublicState = LandClaimArea.GetPublicState(area);
                var otherGroup = areaPublicState.LandClaimAreasGroup;
                if (otherGroup != group)
                {
                    continue;
                }

                hasBounds = true;
                var bounds = SharedCalculateLandClaimAreaBounds(area);
                if (addGraceAreaPadding)
                {
                    bounds = bounds.Inflate(areaPublicState.LandClaimGraceAreaPaddingSizeOneDirection);
                }

                minX = Math.Min(minX, (ushort)Math.Min(bounds.X,                 ushort.MaxValue));
                minY = Math.Min(minY, (ushort)Math.Min(bounds.Y,                 ushort.MaxValue));
                maxX = Math.Max(maxX, (ushort)Math.Min(bounds.X + bounds.Width,  ushort.MaxValue));
                maxY = Math.Max(maxY, (ushort)Math.Min(bounds.Y + bounds.Height, ushort.MaxValue));
            }

            if (!hasBounds)
            {
                return default;
            }

            return new RectangleInt(x: minX,
                                    y: minY,
                                    width: maxX - minX,
                                    height: maxY - minY);
        }

        public static bool SharedIsAreasGroupUnderRaid(ILogicObject areasGroup)
        {
            var groupPublicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            if (!groupPublicState.LastRaidTime.HasValue)
            {
                // not under raid
                return false;
            }

            var time = Api.IsClient
                           ? Client.CurrentGame.ServerFrameTimeRounded
                           : Server.Game.FrameTime;

            var timeSinceRaidStart = time - groupPublicState.LastRaidTime.Value;
            return timeSinceRaidStart < RaidBlockDurationSeconds;
        }

        /// <summary>
        /// Returns true if the area is under the raid.
        /// Client could determine this only if it's own the area (has the private state).
        /// </summary>
        public static bool SharedIsAreaUnderRaid(ILogicObject area)
        {
            var areaPublicState = LandClaimArea.GetPublicState(area);
            var areasGroup = areaPublicState.LandClaimAreasGroup;
            return SharedIsAreasGroupUnderRaid(areasGroup);
        }

        public static bool SharedIsFoundedArea(ILogicObject area, ICharacter forCharacter)
        {
            if (area == null)
            {
                Logger.Warning(nameof(ServerIsOwnedArea) + " - argument area is null");
                return false;
            }

            if (IsClient && !area.ClientHasPrivateState)
            {
                // not an owner so not a founder for sure
                return false;
            }

            var privateState = LandClaimArea.GetPrivateState(area);
            return privateState.LandClaimFounder == forCharacter.Name;
        }

        public static bool SharedIsLandClaimedByAnyone(in Vector2Ushort tilePosition)
        {
            foreach (var area in sharedLandClaimAreas)
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (areaBounds.Contains(tilePosition))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets only a single found area in bounds.
        /// </summary>
        public static bool SharedIsLandClaimedByAnyone(RectangleInt bounds)
        {
            foreach (var area in sharedLandClaimAreas)
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (areaBounds.IntersectsLoose(bounds))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool SharedIsObjectInsideAnyArea(IStaticWorldObject worldObject)
        {
            return SharedIsLandClaimedByAnyone(worldObject.Bounds);
        }

        public static bool SharedIsObjectInsideOwnedOrFreeArea(IStaticWorldObject worldObject, ICharacter who)
        {
            var foundAnyAreas = false;
            var startTilePosition = worldObject.TilePosition;
            var worldObjectLayoutTileOffsets = worldObject.ProtoStaticWorldObject.Layout.TileOffsets;

            foreach (var area in sharedLandClaimAreas)
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                foreach (var tileOffset in worldObjectLayoutTileOffsets)
                {
                    var tilePosition = startTilePosition.AddAndClamp(tileOffset);
                    if (!areaBounds.Contains(tilePosition))
                    {
                        // the object is not inside this area
                        continue;
                    }

                    if (SharedIsOwnedArea(area, who))
                    {
                        // player own this area
                        return true;
                    }

                    foundAnyAreas = true;
                }
            }

            // return true only if there are no areas (free land)
            return !foundAnyAreas;
        }

        public static bool SharedIsOwnedArea(ILogicObject area, ICharacter forCharacter)
        {
            return IsServer
                       ? ServerIsOwnedArea(area, forCharacter)
                       : ClientIsOwnedArea(area);
        }

        public static bool SharedIsOwnedLand(
            Vector2Ushort tilePosition,
            ICharacter who,
            out ILogicObject ownedArea)
        {
            foreach (var area in sharedLandClaimAreas)
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (!areaBounds.Contains(tilePosition))
                {
                    // the object is not inside this area
                    continue;
                }

                if (SharedIsOwnedArea(area, who))
                {
                    // player own this area
                    ownedArea = area;
                    return true;
                }
            }

            // there are no areas or player don't own any area there
            ownedArea = null;
            return false;
        }

        public static bool SharedIsPositionInsideOwnedOrFreeArea(Vector2Ushort tilePosition, ICharacter who)
        {
            var foundAnyAreas = false;
            foreach (var area in sharedLandClaimAreas)
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (!areaBounds.Contains(tilePosition))
                {
                    // the tile position is not inside this area
                    continue;
                }

                if (SharedIsOwnedArea(area, who))
                {
                    // player own this area
                    return true;
                }

                foundAnyAreas = true;
            }

            // return true only if there are no areas (free land)
            return !foundAnyAreas;
        }

        public static bool SharedIsUnderRaidBlock(ICharacter character, IStaticWorldObject staticWorldObject)
        {
            var world = IsClient
                            ? (IWorldService)Client.World
                            : Server.World;

            var protoStaticWorldObject = staticWorldObject.ProtoStaticWorldObject;
            var startTilePosition = staticWorldObject.TilePosition;
            var validatorNoRaid = ValidatorNoRaid;

            foreach (var tileOffset in protoStaticWorldObject.Layout.TileOffsets)
            {
                var occupiedTile = world.GetTile(
                    startTilePosition.X + tileOffset.X,
                    startTilePosition.Y + tileOffset.Y,
                    logOutOfBounds: false);

                if (!occupiedTile.IsValidTile)
                {
                    continue;
                }

                if (validatorNoRaid.Function(
                    new ConstructionTileRequirements.Context(
                        occupiedTile,
                        character,
                        protoStaticWorldObject,
                        tileOffset)))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public static void SharedSendNotificationActionForbiddenUnderRaidblock(ICharacter character)
        {
            if (IsClient)
            {
                Instance.ClientRemote_ShowNotificationCannotUnstuckUnderRaidblock();
            }
            else
            {
                Instance.CallClient(character,
                                    _ => _.ClientRemote_ShowNotificationCannotUnstuckUnderRaidblock());
            }
        }

        protected override void PrepareSystem()
        {
            if (IsServer)
            {
                ServerCharacters.PlayerNameChanged += ServerPlayerNameChangedHandler;
            }
        }

        /// <summary>
        /// Checks the area land claims, release those that do not belong to the group anymore.
        /// Destroys group if it's empty.
        /// </summary>
        private static void ServerCleanupAreasGroup(ILogicObject areasGroup)
        {
            if (areasGroup == null
                || areasGroup.IsDestroyed)
            {
                return;
            }

            using var tempRemovedAreas = Api.Shared.GetTempList<ILogicObject>();
            var areas = LandClaimAreasGroup.GetPrivateState(areasGroup).ServerLandClaimsAreas;
            for (var index = 0; index < areas.Count; index++)
            {
                var area = areas[index];
                if (!area.IsDestroyed
                    && areasGroup == LandClaimArea.GetPublicState(area).LandClaimAreasGroup)
                {
                    continue;
                }

                // this area now doesn't belong to the areas group
                areas.RemoveAt(index);
                index--;
                Logger.Important($"Land claim areas group {areasGroup} - released area: {area}");
                tempRemovedAreas.Add(area);
                Api.SafeInvoke(() => ServerAreaGroupChanged?.Invoke(area, areasGroup, null));
            }

            if (areas.Count > 0)
            {
                return;
            }

            // areas group has no areas which is using it
            Api.SafeInvoke(() => ServerAreasGroupDestroyed?.Invoke(areasGroup));
            Logger.Important("Destroying land claim areas group - not used anymore: " + areasGroup);
            Api.Server.World.DestroyObject(areasGroup);

            foreach (var area in tempRemovedAreas)
            {
                if (area.IsDestroyed)
                {
                    continue;
                }

                var otherAreasGroup = LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
                if (otherAreasGroup == null)
                {
                    continue;
                }

                LandClaimAreasGroup.ServerOnBaseMerged(areasGroup, otherAreasGroup);
                // that's correct! we return here as the base might be merged only with a single other base
                return;
            }
        }

        private static void ServerDropItems(IStaticWorldObject landClaimStructure, ILogicObject areasGroup)
        {
            // try drop items from the safe storage
            var itemsContainer = LandClaimAreasGroup.GetPrivateState(areasGroup).ItemsContainer;
            if (itemsContainer.OccupiedSlotsCount == 0)
            {
                // no items to drop
                return;
            }

            var groundContainer = ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                landClaimStructure.OccupiedTile,
                itemsContainer);

            if (groundContainer == null)
            {
                // no items dropped
                return;
            }

            // set custom timeout for the dropped ground items container
            ObjectGroundItemsContainer.ServerSetDestructionTimeout(
                (IStaticWorldObject)groundContainer.Owner,
                DestroyedLandClaimDroppedItemsDestructionTimeout.TotalSeconds);
        }

        private static void ServerEstablishAreasGroup(RectangleInt bounds)
        {
            // First, try to find all the connected areas (and their groups) within the bounds.
            using var tempListAreas = Api.Shared.GetTempList<ILogicObject>();
            using var tempListGroups = Api.Shared.GetTempList<ILogicObject>();
            SharedGatherAreasAndGroups(bounds, tempListAreas, tempListGroups);

            // Obviously, all the areas there now will belong to the same group.
            // The rest groups will be destroyed.
            // For a candidate for the group let's take the currently existing group
            // with the max number of the land claims.
            var currentGroup = tempListGroups.MaximumOrDefault(
                g => LandClaimAreasGroup.GetPrivateState(g).ServerLandClaimsAreas.Count);

            if (currentGroup == null)
            {
                // need to create a new group
                currentGroup = Api.Server.World.CreateLogicObject<LandClaimAreasGroup>();
                Logger.Important("Created land claim areas group: " + currentGroup);

                Api.SafeInvoke(() => ServerAreasGroupCreated?.Invoke(currentGroup));
            }

            var groupPrivateState = LandClaimAreasGroup.GetPrivateState(currentGroup);
            if (groupPrivateState.ServerLandClaimsAreas == null)
            {
                groupPrivateState.ServerLandClaimsAreas = new List<ILogicObject>();
            }

            var areasGroupAreasList = groupPrivateState.ServerLandClaimsAreas;

            // assign the group to all the land claim areas there
            foreach (var area in tempListAreas)
            {
                var areaPublicState = LandClaimArea.GetPublicState(area);
                var areaGroup = areaPublicState.LandClaimAreasGroup;
                if (areaGroup == currentGroup)
                {
                    continue;
                }

                areaPublicState.LandClaimAreasGroup = currentGroup;
                Logger.Important($"Assigned land claim areas group: {currentGroup} - to {area}");
                areasGroupAreasList.Add(area);

                if (areaGroup != null)
                {
                    // group changed - copy necessary properties such as last raid time
                    LandClaimAreasGroup.ServerOnGroupChanged(area, areaGroup, currentGroup);
                }

                Api.SafeInvoke(() => ServerAreaGroupChanged?.Invoke(area, areaGroup, currentGroup));
            }

            foreach (var otherGroup in tempListGroups)
            {
                if (currentGroup != otherGroup)
                {
                    ServerCleanupAreasGroup(otherGroup);
                }
            }
        }

        private static void ServerOnAddLandOwner(ILogicObject area, ICharacter playerToAdd, bool notify)
        {
            // add this with some delay to prevent from the bug when the player name listed twice due to the late delta-replication
            if (notify)
            {
                ServerTimersSystem.AddAction(
                    delaySeconds: 0.1,
                    OnAddLandOwner);
            }
            else
            {
                OnAddLandOwner();
            }

            void OnAddLandOwner()
            {
                if (!ServerIsOwnedArea(area, playerToAdd))
                {
                    // The owner is already removed during the short delay? That's was quick!
                    return;
                }
                
                ServerWorld.EnterPrivateScope(playerToAdd, area);
                SharedGetPlayerOwnedAreas(playerToAdd).AddIfNotContains(area);

                if (notify)
                {
                    Instance.CallClient(
                        playerToAdd,
                        _ => _.ClientRemote_OnLandOwnerStateChanged(area, true));
                }
            }
        }

        private static void ServerOnRemoveLandOwner(ILogicObject area, ICharacter removedPlayer)
        {
            InteractableStaticWorldObjectHelper.ServerTryAbortInteraction(
                removedPlayer,
                LandClaimArea.GetPrivateState(area).ServerLandClaimWorldObject);

            ServerWorld.ExitPrivateScope(removedPlayer, area);
            SharedGetPlayerOwnedAreas(removedPlayer).Remove(area);

            Instance.CallClient(removedPlayer, _ => _.ClientRemote_OnLandOwnerStateChanged(area, false));
        }

        private static void ServerPlayerNameChangedHandler(string oldName, string newName)
        {
            foreach (var area in sharedLandClaimAreas)
            {
                var privateState = LandClaimArea.GetPrivateState(area);
                if (string.Equals(privateState.LandClaimFounder, oldName, StringComparison.Ordinal))
                {
                    privateState.LandClaimFounder = newName;
                    Logger.Important(
                        $"Replaced founder owner entry for land claim: {oldName}->{newName} in {area}, {privateState.ServerLandClaimWorldObject}");
                }

                // find and rename access list entry for this player
                var owners = privateState.LandOwners;
                if (owners == null)
                {
                    continue;
                }

                for (var index = 0; index < owners.Count; index++)
                {
                    var owner = owners[index];
                    if (!string.Equals(owner, oldName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    // replace owner entry
                    owners.RemoveAt(index);
                    owners.Insert(index, newName);
                    Logger.Important(
                        $"Replaced access owner entry: {oldName}->{newName} in {area}, {privateState.ServerLandClaimWorldObject}");
                    break;
                }
            }
        }

        private static void ServerTryRebuildLandClaimsGroups(RectangleInt bounds)
        {
            using var tempListAreas = Api.Shared.GetTempList<ILogicObject>();
            using var tempListGroups = Api.Shared.GetTempList<ILogicObject>();
            SharedGatherAreasAndGroups(bounds, tempListAreas, tempListGroups);

            foreach (var areasGroup in tempListGroups)
            {
                ServerReleaseGroupUnconnectedAreas(areasGroup);
            }

            // rebuild groups for every land claim without a group there
            foreach (var otherArea in tempListAreas)
            {
                if (LandClaimArea.GetPublicState(otherArea).LandClaimAreasGroup == null)
                {
                    ServerEstablishAreasGroup(
                        SharedCalculateLandClaimAreaBounds(otherArea));
                }
            }

            void ServerReleaseGroupUnconnectedAreas(ILogicObject areasGroup)
            {
                ServerCleanupAreasGroup(areasGroup);

                var currentAreas = LandClaimAreasGroup.GetPrivateState(areasGroup).ServerLandClaimsAreas;
                using var tempListCurrentGroupAreas = Api.Shared.GetTempList<ILogicObject>();
                SharedGetConnectedAreas(tempListCurrentGroupAreas.AsList(),
                                        SharedCalculateLandClaimAreaBounds(currentAreas[0]));

                for (var index = 0; index < currentAreas.Count; index++)
                {
                    var area = currentAreas[index];
                    if (tempListCurrentGroupAreas.Contains(area))
                    {
                        continue;
                    }

                    // this area is no longer belongs to this group
                    currentAreas.RemoveAt(index);
                    index--;

                    LandClaimArea.GetPublicState(area).LandClaimAreasGroup = null;
                    Logger.Important($"Reset land claim areas group: null - to {area}");
                }
            }
        }

        private static void SharedGatherAreasAndGroups(
            RectangleInt bounds,
            ITempList<ILogicObject> tempListAreas,
            ITempList<ILogicObject> tempListGroups)
        {
            SharedGetConnectedAreas(tempListAreas.AsList(), bounds);

            // gather all groups for the areas found there
            foreach (var area in tempListAreas)
            {
                var areasGroup = LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
                if (areasGroup != null)
                {
                    tempListGroups.AddIfNotContains(areasGroup);
                }
            }
        }

        private static void SharedGetConnectedAreas(List<ILogicObject> result, RectangleInt bounds)
        {
            // find areas in bounds
            foreach (var area in sharedLandClaimAreas)
            {
                if (area.IsDestroyed)
                {
                    continue;
                }

                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (areaBounds.Intersects(bounds))
                {
                    result.Add(area);
                }
            }

            // collect adjacent areas
            for (var index = 0; index < result.Count; index++)
            {
                var currentArea = result[index];
                var currentAreaBounds = SharedCalculateLandClaimAreaBounds(currentArea);
                // find adjacent areas
                foreach (var otherArea in sharedLandClaimAreas)
                {
                    var otherAreaBounds = SharedGetLandClaimAreaBounds(otherArea);
                    if (otherAreaBounds.Intersects(currentAreaBounds)
                        && !result.Contains(otherArea))
                    {
                        // found new area - it will be also checked for adjacent neighbors
                        result.Add(otherArea);
                    }
                }
            }
        }

        private static NetworkSyncList<ILogicObject> SharedGetPlayerOwnedAreas(ICharacter player)
        {
            return PlayerCharacter.GetPrivateState(player)
                                  .OwnedLandClaimAreas;
        }

        private void ClientRemote_OnCannotInteractNotOwner(IStaticWorldObject worldObject)
        {
            CannotInteractMessageDisplay.ClientOnCannotInteract(worldObject,
                                                                ErrorNotLandOwner_Message,
                                                                isOutOfRange: false);
        }

        private void ClientRemote_OnLandClaimUpgraded(ILogicObject area)
        {
            ClientLandClaimAreaManager.OnLandClaimUpgraded(area);
        }

        private void ClientRemote_OnLandOwnerStateChanged(ILogicObject area, bool isOwned)
        {
            Logger.Important("Land claim area ownership update: " + area + " isOwned=" + isOwned);
            ClientLandClaimAreaManager.OnLandOwnerStateChanged(area, isOwned);
        }

        private void ClientRemote_ShowNotificationCannotUnstuckUnderRaidblock()
        {
            NotificationSystem.ClientShowNotification(
                CoreStrings.Notification_ActionForbidden,
                ErrorRaidBlockActionRestricted_Message,
                color: NotificationColor.Bad);
        }

        private LandClaimsGroupDecayInfo ServerRemote_GetDecayInfo(IStaticWorldObject landClaimStructure)
        {
            var character = ServerRemoteContext.Character;
            if (!Server.World.IsInPrivateScope(landClaimStructure, character))
            {
                throw new Exception("Cannot interact with the land claim object is not in private scope: "
                                    + landClaimStructure);
            }

            var area = ServerGetLandClaimArea(landClaimStructure);
            var areasGroup = LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
            var areas = LandClaimAreasGroup.GetPrivateState(areasGroup).ServerLandClaimsAreas;
            var decayDelayDuration = ServerGetDecayDelayDurationForLandClaimAreas(areas);

            decayDelayDuration = Math.Max(decayDelayDuration,
                                          StructureConstants.StructuresAbandonedDecayDelaySeconds);

            return new LandClaimsGroupDecayInfo(decayDelayDuration,
                                                decayDuration: StructureConstants.StructuresDecayDurationSeconds);
        }

        private void ServerRemote_RequestOwnedAreas()
        {
            var character = ServerRemoteContext.Character;
            // add to the character private scope all owned areas
            foreach (var area in SharedGetPlayerOwnedAreas(character))
            {
                ServerWorld.EnterPrivateScope(character, area);
            }
        }

        private string ServerRemote_SetAreaOwners(ILogicObject area, List<string> newOwners)
        {
            var owner = ServerRemoteContext.Character;

            if (!Server.World.IsInPrivateScope(area, owner))
            {
                throw new Exception("Cannot interact with the land claim object as the area is not in private scope: "
                                    + area);
            }

            var privateState = LandClaimArea.GetPrivateState(area);
            var currentOwners = privateState.LandOwners;

            if (!currentOwners.Contains(owner.Name)
                && !CreativeModeSystem.SharedIsInCreativeMode(owner))
            {
                return WorldObjectOwnersSystem.DialogCannotSetOwners_MessageNotOwner;
            }

            if (!((IProtoObjectLandClaim)privateState.ServerLandClaimWorldObject.ProtoStaticWorldObject)
                    .SharedCanEditOwners(privateState.ServerLandClaimWorldObject, owner))
            {
                return WorldObjectOwnersSystem.DialogCannotSetOwners_MessageCannotEdit;
            }

            currentOwners.GetDiff(newOwners, out var ownersToAdd, out var ownersToRemove);
            if (currentOwners.Count - ownersToRemove.Count <= 0)
            {
                return WorldObjectOwnersSystem.DialogCannotSetOwners_MessageCannotRemoveLastOwner;
            }

            if (ownersToRemove.Contains(owner.Name)
                && !CreativeModeSystem.SharedIsInCreativeMode(owner))
            {
                return WorldObjectOwnersSystem.DialogCannotSetOwners_MessageCannotRemoveSelf;
            }

            foreach (var n in ownersToAdd)
            {
                var name = n;
                var playerToAdd = ServerCharacters.GetPlayerCharacter(name);
                if (playerToAdd == null)
                {
                    return string.Format(WorldObjectOwnersSystem.DialogCannotSetOwners_MessageFormatPlayerNotFound,
                                         name);
                }

                // get proper player name
                name = playerToAdd.Name;
                if (currentOwners.AddIfNotContains(name))
                {
                    Logger.Important($"Added land owner: {name}, land: {area}", characterRelated: owner);
                    ServerOnAddLandOwner(area, playerToAdd, notify: true);
                }
            }

            foreach (var name in ownersToRemove)
            {
                if (!currentOwners.Remove(name))
                {
                    continue;
                }

                Logger.Important($"Removed land owner: {name}, land: {area}", characterRelated: owner);

                var removedPlayer = ServerCharacters.GetPlayerCharacter(name);
                if (removedPlayer == null)
                {
                    continue;
                }

                ServerOnRemoveLandOwner(area, removedPlayer);
            }

            return null;
        }

        public readonly struct LandClaimsGroupDecayInfo : IRemoteCallParameter
        {
            public readonly double DecayDelayDuration;

            public readonly double DecayDuration;

            public LandClaimsGroupDecayInfo(double decayDelayDuration, double decayDuration)
            {
                this.DecayDelayDuration = decayDelayDuration;
                this.DecayDuration = decayDuration;
            }
        }

        [PrepareOrder(afterType: typeof(BootstrapperServerCore))]
        public class BootstrapperLandClaimSystem : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                var world = Api.Client.World;
                world.WorldBoundsChanged += ClientReset;
                world.ObjectEnterScope += ClientObjectEnterScopeHandler;
                world.ObjectLeftScope += ClientObjectLeftScopeHandler;
                ClientReset();

                Client.Characters.CurrentPlayerCharacterChanged += ClientTryRequestOwnedAreas;

                ClientTryRequestOwnedAreas();

                static void ClientTryRequestOwnedAreas()
                {
                    if (Api.Client.Characters.CurrentPlayerCharacter == null)
                    {
                        return;
                    }

                    Logger.Important("Land claim areas requested from server");
                    Instance.CallServer(_ => _.ServerRemote_RequestOwnedAreas());
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                Server.World.WorldBoundsChanged += ServerWorldBoundsChangedHandler;
                ServerLoadSystem();

                static void ServerWorldBoundsChangedHandler()
                {
                    const string key = nameof(LandClaimAreaManager);
                    Server.Database.Remove(key, key);
                    Server.World.DestroyObject(serverLandClaimManagerInstance);
                    ServerLoadSystem();
                }
            }

            private static void ClientObjectEnterScopeHandler(IGameObjectWithProto obj)
            {
                if (obj is ILogicObject logicObject
                    && obj.ProtoGameObject is LandClaimArea)
                {
                    sharedLandClaimAreas.Add(logicObject);
                    ClientLandClaimAreaManager.AddArea(logicObject);
                }
            }

            private static void ClientObjectLeftScopeHandler(IGameObjectWithProto obj)
            {
                if (obj is ILogicObject logicObject
                    && obj.ProtoGameObject is LandClaimArea)
                {
                    sharedLandClaimAreas.Remove(logicObject);
                    ClientLandClaimAreaManager.RemoveArea(logicObject);
                }
            }

            private static void ClientReset()
            {
                // remove old areas
                foreach (var area in sharedLandClaimAreas)
                {
                    ClientLandClaimAreaManager.RemoveArea(area);
                }

                sharedLandClaimAreas.Clear();

                // add current areas
                foreach (var area in Api.Client.World.FindGameObjectsOfProto<ILogicObject, LandClaimArea>())
                {
                    sharedLandClaimAreas.Add(area);
                    ClientLandClaimAreaManager.AddArea(area);
                }
            }

            private static void ServerLoadSystem()
            {
                const string key = nameof(LandClaimAreaManager);
                if (Server.Database.TryGet(key, key, out ILogicObject savedManager))
                {
                    Server.World.DestroyObject(savedManager);
                }

                serverLandClaimManagerInstance = Server.World.CreateLogicObject<LandClaimAreaManager>();
                var publicState = LandClaimAreaManager.GetPublicState(serverLandClaimManagerInstance);
                publicState.LandClaimAreas = new NetworkSyncList<ILogicObject>();

                Server.Database.Set(key, key, serverLandClaimManagerInstance);

                sharedLandClaimAreas = LandClaimAreaManager.GetPublicState(serverLandClaimManagerInstance)
                                                           .LandClaimAreas;

                foreach (var area in sharedLandClaimAreas)
                {
                    var areaPrivateState = LandClaimArea.GetPrivateState(area);
                    var areaPublicState = LandClaimArea.GetPublicState(area);
                    areaPublicState.SetupAreaProperties(areaPrivateState);
                }
            }
        }
    }
}