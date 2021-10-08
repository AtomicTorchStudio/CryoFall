namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Perks;
    using AtomicTorch.CBND.CoreMod.Perks.Base;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Deconstruction;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.RaidingProtection;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.StructureDecaySystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public partial class LandClaimSystem : ProtoSystem<LandClaimSystem>
    {
        public const string ErrorCannotBuild_AnotherLandClaimTooClose =
            "Too close to another land claim.";

        public const string ErrorCannotBuild_AreaIsClaimedOrTooCloseToClaimed =
            "The area is claimed by another player, or it's too close to someone else's land claim.";

        public const string ErrorCannotBuild_BaseSizeExceeded_Format =
            "Base size exceeded. Maximum base size in any dimension is {0} cells (approximately {1} land claims stacked together in a row).";

        public const string ErrorCannotBuild_DemoPlayerLandClaims =
            "Cannot combine land claims. In order to prevent abuse/exploits, land claims belonging to demo accounts cannot be combined with other players'.";

        // The game has a resource contesting system preventing the players
        // to claim or build near the recently spawned oil and other resource deposits.
        public const string ErrorCannotBuild_DepositCooldown =
            "The resource deposit has only just appeared and cannot be claimed yet. Construction around it is temporary restricted.";

        public const string ErrorCannotBuild_ExceededSafeStorageCapacity =
            "Building/upgrading this land claim will result in combined safe storage exceeding the possible capacity in a base. Please remove items from the safe storages of the neighboring land claims and then repeat the action.";

        public const string ErrorCannotBuild_IntersectingWithAnotherLandClaim =
            "Intersecting with another land claim area";

        public const string ErrorCannotBuild_IntersectingWithAnotherLandClaimUnderShieldProtection =
            "Intersecting with another land claim area that is under active shield protection. Disable the shield there first.";

        public const string ErrorCannotBuild_LandClaimAmountLimitCanIncrease_Format =
            "You can increase the limit by researching technologies for higher-tier land claims (up to {0} total).";

        public const string ErrorCannotBuild_LandClaimAmountLimitExceeded_Format =
            "You've used up your allotted number of personal land claims ({0}/{1}).";

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

        private const int MaxNumberOfLandClaimsInRow = 3;

        public static readonly Lazy<ushort> MaxLandClaimSize
            = new(() => Api.FindProtoEntities<IProtoObjectLandClaim>()
                           .Max(l => l.LandClaimSize));

        private static ILogicObject serverLandClaimManagerInstance;

        private static IList<ILogicObject> sharedLandClaimAreas
            = IsClient
                  ? new List<ILogicObject>() // workaround for client so it will be able to query (empty) list
                  : null;

        public static readonly ConstructionTileRequirements.Validator ValidatorIsOwnedOrFreeLand
            = new(check: context => ValidatorIsOwnedOrFreeLandCheck(context,
                                                                    requireFactionPermission: true,
                                                                    out _),
                  getErrorMessage:
                  context =>
                  {
                      ValidatorIsOwnedOrFreeLandCheck(context,
                                                      requireFactionPermission: true,
                                                      out var hasNoFactionPermission);
                      return hasNoFactionPermission
                                 ? string.Format(CoreStrings.Faction_Permission_Required_Format,
                                                 CoreStrings.Faction_Permission_LandClaimManagement_Title)
                                 : ErrorCannotBuild_AreaIsClaimedOrTooCloseToClaimed;
                  });

        public static readonly ConstructionTileRequirements.Validator
            ValidatorIsOwnedOrFreeLandNoFactionPermissionsRequired
                = new(ErrorCannotBuild_AreaIsClaimedOrTooCloseToClaimed,
                      context => ValidatorIsOwnedOrFreeLandCheck(context, requireFactionPermission: false, out _));

        /// <summary>
        /// Special validator for cases when the server needs to spawn event objects (such as
        /// space debris, meteorites, etc) to ensure they will not spawn inside player land claims
        /// (including their grace area).
        /// </summary>
        public static readonly ConstructionTileRequirements.Validator ValidatorFreeLandEvenForServer
            = new( // reuse this error message as it matches this validator well
                // (technically this validation message will be never visible to player)
                ErrorCannotBuild_AnotherLandClaimTooClose,
                context =>
                {
                    var forCharacter = context.CharacterBuilder;
                    if (forCharacter is not null
                        && CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                    {
                        return true;
                    }

                    var position = context.Tile.Position;

                    foreach (var area in SharedGetLandClaimAreasCache().EnumerateForPosition(position))
                    {
                        var areaBoundsDirect = SharedGetLandClaimAreaBounds(area);
                        var areaBoundsWithPadding = areaBoundsDirect.Inflate(LandClaimArea.GetPublicState(area)
                                                                                 .ProtoObjectLandClaim
                                                                                 .LandClaimGraceAreaPaddingSizeOneDirection);

                        if (areaBoundsWithPadding.Contains(position))
                        {
                            return false;
                        }
                    }

                    return true;
                });

        public static readonly ConstructionTileRequirements.Validator ValidatorIsOwnedLand
            = new(check: context => ValidatorIsOwnedLandCheck(context,
                                                              requireFactionPermission: true,
                                                              out _),
                  getErrorMessage:
                  context =>
                  {
                      ValidatorIsOwnedLandCheck(context,
                                                requireFactionPermission: true,
                                                out var hasNoFactionPermission);
                      return hasNoFactionPermission
                                 ? string.Format(CoreStrings.Faction_Permission_Required_Format,
                                                 CoreStrings.Faction_Permission_LandClaimManagement_Title)
                                 : ErrorNotLandOwner_Message;
                  });

        public static readonly ConstructionTileRequirements.Validator ValidatorIsOwnedLandInPvEOnly
            = new(check: context =>
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

                             if (!PveSystem.SharedIsPve(false))
                             {
                                 // in PvP only check whether the land is not claimed by another player
                                 return ValidatorIsOwnedOrFreeLand.CheckFunction.Invoke(context);
                             }

                             return SharedIsOwnedLand(context.Tile.Position,
                                                      forCharacter,
                                                      requireFactionPermission: true,
                                                      out _);
                         },
                  getErrorMessage:
                  context =>
                  {
                      SharedIsOwnedLand(context.Tile.Position,
                                        context.CharacterBuilder,
                                        requireFactionPermission: true,
                                        out var hasNoFactionPermission,
                                        out _);

                      if (hasNoFactionPermission)
                      {
                          return string.Format(CoreStrings.Faction_Permission_Required_Format,
                                               CoreStrings.Faction_Permission_LandClaimManagement_Title);
                      }

                      return PveSystem.SharedIsPve(false)
                                 ? ErrorCannotBuild_RequiresOwnedArea
                                 : ErrorNotLandOwner_Message;
                  });

        public static readonly ConstructionTileRequirements.Validator ValidatorNoRaid
            = new(ErrorCannotBuild_RaidUnderWay,
                  context =>
                  {
                      var forCharacter = context.CharacterBuilder;
                      if (forCharacter is null)
                      {
                          return true;
                      }

                      var position = context.Tile.Position;
                      foreach (var area in SharedGetLandClaimAreasCache().EnumerateForPosition(position))
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

        public static readonly ConstructionTileRequirements.Validator ValidatorNoShieldProtection
            = new(CoreStrings.ShieldProtection_ActionRestrictedBaseUnderShieldProtection,
                  context =>
                  {
                      var forCharacter = context.CharacterBuilder;
                      if (forCharacter is null)
                      {
                          return true;
                      }

                      var position = context.Tile.Position;
                      foreach (var area in SharedGetLandClaimAreasCache().EnumerateForPosition(position))
                      {
                          var areaBounds = SharedGetLandClaimAreaBounds(area, addGracePadding: true);
                          if (!areaBounds.Contains(position))
                          {
                              continue;
                          }

                          // only server and client owning the area has the private state of the area
                          // to check whether it's under raid or not
                          if (LandClaimShieldProtectionSystem.SharedIsAreaUnderShieldProtection(area))
                          {
                              // cannot build - there is an area under shield protection
                              return false;
                          }
                      }

                      return true;
                  });

        public static readonly ConstructionTileRequirements.Validator ValidatorNewLandClaimNoLandClaimIntersections
            = new(context => ValidatorNewLandClaimNoLandClaimIntersectionsCheck(context, out _),
                  getErrorMessage:
                  context =>
                  {
                      ValidatorNewLandClaimNoLandClaimIntersectionsCheck(context, out var hasNoFactionPermission);
                      return hasNoFactionPermission
                                 ? string.Format(CoreStrings.Faction_Permission_Required_Format,
                                                 CoreStrings.Faction_Permission_LandClaimManagement_Title)
                                 : ErrorCannotBuild_IntersectingWithAnotherLandClaim;
                  });

        public static readonly ConstructionTileRequirements.Validator
            ValidatorNewLandClaimNoLandClaimIntersectionsWithShieldProtection
                = new(ErrorCannotBuild_IntersectingWithAnotherLandClaimUnderShieldProtection,
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

                          var protoObjectLandClaim = (IProtoObjectLandClaim)context.ProtoStaticObjectToBuild;
                          if (context.TileOffset
                              != SharedCalculateLandClaimObjectCenterTilePosition(
                                  Vector2Ushort.Zero,
                                  protoObjectLandClaim))
                          {
                              // we don't check offset tiles
                              // as the land claim area calculated from the center tile of the land claim object
                              return true;
                          }

                          var centerTilePosition = context.Tile.Position;
                          return SharedCheckCanPlaceOrUpgradeLandClaimThereConsideringShieldProtection(
                              protoObjectLandClaim,
                              centerTilePosition,
                              forCharacter);
                      });

        public static readonly ConstructionTileRequirements.Validator
            ValidatorNewLandClaimSafeStorageCapacityNotExceeded
                = new(ErrorCannotBuild_ExceededSafeStorageCapacity,
                      context =>
                      {
                          if (IsClient)
                          {
                              // client cannot perform this check
                              return true;
                          }

                          var forCharacter = context.CharacterBuilder;
                          if (forCharacter is null)
                          {
                              return true;
                          }

                          if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                          {
                              return true;
                          }

                          var protoObjectLandClaim = (IProtoObjectLandClaim)context.ProtoStaticObjectToBuild;
                          if (context.TileOffset
                              != SharedCalculateLandClaimObjectCenterTilePosition(
                                  Vector2Ushort.Zero,
                                  protoObjectLandClaim))
                          {
                              // we don't check offset tiles
                              // as the land claim area calculated from the center tile of the land claim object
                              return true;
                          }

                          var centerTilePosition = context.Tile.Position;
                          return ServerCheckFutureBaseWillNotExceedSafeStorageCapacity(protoObjectLandClaim,
                              centerTilePosition,
                              forCharacter);
                      });

        public static readonly ConstructionTileRequirements.Validator
            ValidatorNewLandClaimFactionLandClaimLimitNotExceeded
                = new(CoreStrings.Faction_LandClaimNumberLimit_Reached
                      + "[br]"
                      + CoreStrings.Faction_LandClaimNumberLimit_CanIncrease,
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

                          var protoObjectLandClaim = (IProtoObjectLandClaim)context.ProtoStaticObjectToBuild;
                          if (context.TileOffset
                              != SharedCalculateLandClaimObjectCenterTilePosition(
                                  Vector2Ushort.Zero,
                                  protoObjectLandClaim))
                          {
                              // we don't check offset tiles
                              // as the land claim area calculated from the center tile of the land claim object
                              return true;
                          }

                          var centerTilePosition = context.Tile.Position;
                          return SharedCheckFutureBaseWillNotExceedFactionLandClaimLimit(protoObjectLandClaim,
                              centerTilePosition,
                              forCharacter,
                              isForNewLandClaim: true);
                      });

        public static readonly ConstructionTileRequirements.Validator ValidatorCheckLandClaimBaseSizeLimitNotExceeded
            = new(() => string.Format(ErrorCannotBuild_BaseSizeExceeded_Format,
                                      SharedGetMaxBaseSizeTiles(),
                                      MaxNumberOfLandClaimsInRow),
                  context =>
                  {
                      var forCharacter = context.CharacterBuilder;
                      if (forCharacter is null)
                      {
                          return true;
                      }

                      // as players want to check this limit in Editor (to plan a base),
                      // it's enabled even for creative mode players there
                      if (!Api.IsEditor
                          && CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
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
                      var newAreaBounds = SharedCalculateLandClaimAreaBounds(
                          centerTilePosition,
                          protoObjectLandClaim.LandClaimWithGraceAreaSize);

                      // deflate by 1 tile as bases have 1 tile buffer area around them that allows intersection
                      newAreaBounds = newAreaBounds.Inflate(-1);

                      using var tempListAreas = Api.Shared.GetTempList<ILogicObject>();
                      using var tempListGroups = Api.Shared.GetTempList<ILogicObject>();
                      SharedGatherAreasAndGroups(newAreaBounds,
                                                 tempListAreas,
                                                 tempListGroups,
                                                 addGracePaddingToEachArea: true);

                      if (tempListGroups.Count == 0)
                      {
                          // a new base will be created
                          return true;
                      }

                      // a base will be expanded or grouped
                      // calculate the future base size and determine whether it could be exceeding the allowed max size
                      int startX = int.MaxValue,
                          startY = int.MaxValue,
                          endX = 0,
                          endY = 0;

                      // include max bounds of the future land claim
                      UpdateBounds(
                          SharedCalculateLandClaimAreaBounds(context.Tile.Position,
                                                             MaxLandClaimSizeWithGraceArea.Value));

                      foreach (var area in tempListAreas.AsList())
                      {
                          var bounds = SharedGetLandClaimAreaBounds(area, addGracePadding: true);
                          UpdateBounds(bounds);
                      }

                      void UpdateBounds(RectangleInt bounds)
                      {
                          if (bounds.Left < startX)
                          {
                              startX = bounds.Left;
                          }

                          if (bounds.Bottom < startY)
                          {
                              startY = bounds.Bottom;
                          }

                          if (bounds.Right > endX)
                          {
                              endX = bounds.Right;
                          }

                          if (bounds.Top > endY)
                          {
                              endY = bounds.Top;
                          }
                      }

                      var newBaseBounds = new BoundsInt(startX, startY, endX, endY);
                      var maxBaseSizeTiles = SharedGetMaxBaseSizeTiles();
                      return newBaseBounds.Size.X <= maxBaseSizeTiles
                             && newBaseBounds.Size.Y <= maxBaseSizeTiles;
                  });

        public static readonly ConstructionTileRequirements.Validator
            ValidatorNewLandClaimNoLandClaimIntersectionsWithDemoPlayers
                = new(ErrorCannotBuild_DemoPlayerLandClaims,
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

                          var protoObjectLandClaim = (IProtoObjectLandClaim)context.ProtoStaticObjectToBuild;
                          if (context.TileOffset
                              != SharedCalculateLandClaimObjectCenterTilePosition(
                                  Vector2Ushort.Zero,
                                  protoObjectLandClaim))
                          {
                              // we don't check offset tiles
                              // as the land claim area calculated from the center tile of the land claim object
                              return true;
                          }

                          var centerTilePosition = context.Tile.Position;
                          return SharedCheckNoLandClaimByDemoPlayers(protoObjectLandClaim,
                                                                     centerTilePosition,
                                                                     forCharacter,
                                                                     exceptAreasGroup: null);
                      });

        public static readonly ConstructionTileRequirements.Validator ValidatorNewLandClaimNoLandClaimsTooClose
            = new(ErrorCannotBuild_AnotherLandClaimTooClose,
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
            = new(context =>
                  {
                      var forCharacter = context.CharacterBuilder;
                      if (forCharacter is null)
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

                      SharedGetPersonalLandClaimsNumberLimit(forCharacter,
                                                             out var currentNumber,
                                                             out var maxNumber);

                      return maxNumber > currentNumber; // ok only in case when the limit is not exceeded
                  },
                  getErrorMessage:
                  _ =>
                  {
                      if (IsServer)
                      {
                          return string.Empty;
                      }

                      SharedGetPersonalLandClaimsNumberLimit(ClientCurrentCharacterHelper.Character,
                                                             out var currentNumber,
                                                             out var maxNumber);

                      var error = string.Format(ErrorCannotBuild_LandClaimAmountLimitExceeded_Format,
                                                currentNumber,
                                                maxNumber);

                      // find if player has any unresearched tech node than can raise the number of personal land claims
                      var unresearchedTechNodes = TechGroup.AvailableTechGroups.SelectMany(t => t.Nodes)
                                                           .Where(n => n.IsAvailable)
                                                           .Except(ClientCurrentCharacterHelper.PrivateState
                                                                       .Technologies.Nodes);

                      var maxPossibleNumber = maxNumber;
                      var canRaiseLimitNumber = false;
                      foreach (var node in unresearchedTechNodes)
                      {
                          foreach (var effect in node.NodeEffects)
                          {
                              if (effect is TechNodeEffectPerkUnlock
                              {
                                  Perk: ProtoPerkIncreaseLandClaimLimit protoPerk
                              })
                              {
                                  canRaiseLimitNumber = true;
                                  maxPossibleNumber += protoPerk.LimitIncrease;
                                  break;
                              }
                          }
                      }

                      if (canRaiseLimitNumber)
                      {
                          error += "[br]"
                                   + string.Format(ErrorCannotBuild_LandClaimAmountLimitCanIncrease_Format,
                                                   maxPossibleNumber);
                      }

                      return error;
                  });

        public static readonly ConstructionTileRequirements.Validator
            ValidatorCheckLandClaimDepositRequireXenogeology
                = new(ErrorCannotBuild_NeedXenogeologyTech,
                      context =>
                      {
                          var forCharacter = context.CharacterBuilder;
                          if (forCharacter is null)
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

                          if (PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
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
                              if (((IProtoObjectDeposit)mark.ProtoWorldObject).LifetimeTotalDurationSeconds <= 0)
                              {
                                  // infinite oil/Li sources are not considered
                                  continue;
                              }

                              var position = mark.Position;
                              if (position == default)
                              {
                                  var obj = Api.IsServer
                                                ? ServerWorld.GetGameObjectById<IStaticWorldObject>(
                                                    GameObjectType.StaticObject,
                                                    mark.Id)
                                                : ClientWorld.GetGameObjectById<IStaticWorldObject>(
                                                    GameObjectType.StaticObject,
                                                    mark.Id);
                                  if (obj is null)
                                  {
                                      // no object exists (in case of client - could be out of scope)
                                      continue;
                                  }

                                  position = WorldMapResourceMarksSystem.SharedGetObjectCenterPosition(obj);
                              }

                              var staticWorldObjectBounds = new BoundsInt(
                                  position,
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

        public static readonly Lazy<ushort> MinLandClaimSize
            = new(() => Api.FindProtoEntities<IProtoObjectLandClaim>()
                           .Min(l => l.LandClaimSize));

        public static readonly Lazy<ushort> MaxLandClaimSizeWithGraceArea
            = new(() => (ushort)(MaxLandClaimSize.Value + MinPaddingSizeOneDirection * 2));

        public static readonly ConstructionTileRequirements.Validator ValidatorCheckLandClaimDepositClaimDelay
            = new(ErrorCannotBuild_DepositCooldown,
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

                      if (PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                      {
                          return true;
                      }

                      var restrictionSize = DepositResourceLandClaimRestrictionSize.Value;
                      foreach (var mark in WorldMapResourceMarksSystem.SharedEnumerateMarks())
                      {
                          if (((IProtoObjectDeposit)mark.ProtoWorldObject).LifetimeTotalDurationSeconds <= 0)
                          {
                              // infinite oil/Li sources are not considered
                              continue;
                          }

                          var timeRemainsToClaimCooldown =
                              (int)WorldMapResourceMarksSystem.SharedCalculateTimeToClaimLimitRemovalSeconds(
                                  mark.ServerSpawnTime);
                          if (timeRemainsToClaimCooldown <= 0)
                          {
                              continue;
                          }

                          var position = mark.Position;
                          if (position == default)
                          {
                              var obj = Api.IsServer
                                            ? ServerWorld.GetGameObjectById<IStaticWorldObject>(
                                                GameObjectType.StaticObject,
                                                mark.Id)
                                            : ClientWorld.GetGameObjectById<IStaticWorldObject>(
                                                GameObjectType.StaticObject,
                                                mark.Id);
                              if (obj is null)
                              {
                                  // no object exists (in case of client - could be out of scope)
                                  continue;
                              }

                              position = WorldMapResourceMarksSystem.SharedGetObjectCenterPosition(obj);
                          }

                          var bounds = new BoundsInt(
                              position - (restrictionSize.X / 2, restrictionSize.Y / 2),
                              restrictionSize);

                          if (bounds.Contains(context.Tile.Position))
                          {
                              // cannot claim this deposit yet
                              return false;
                          }
                      }

                      return true;
                  });

        public static readonly IWorldClientService ClientWorld
            = IsClient ? Client.World : null;

        private static readonly Lazy<Vector2Ushort> DepositResourceLandClaimRestrictionSize
            = new(() => new Vector2Ushort((ushort)(8 + 2 * MaxLandClaimSizeWithGraceArea.Value),
                                          (ushort)(8 + 2 * MaxLandClaimSizeWithGraceArea.Value)));

        // how long the items dropped on the ground from the safe storage should remain there
        private static readonly TimeSpan DestroyedLandClaimDroppedItemsDestructionTimeout = TimeSpan.FromDays(1);

        private static readonly ICharactersServerService ServerCharacters
            = IsServer ? Server.Characters : null;

        private static readonly IWorldServerService ServerWorld
            = IsServer ? Server.World : null;

        private static LandClaimAreasCache sharedLandClaimAreasCache;

        public delegate void DelegateServerObjectLandClaimDestroyed(
            IStaticWorldObject landClaimStructure,
            RectangleInt areaBounds,
            LandClaimAreaPublicState areaPublicState,
            bool isDestroyedByPlayers,
            bool isDeconstructed);

        public delegate void DelegateServerRaidBlockStartedOrExtended(
            ILogicObject area,
            [CanBeNull] ICharacter raiderCharacter,
            bool isNewRaidBlock,
            bool isStructureDestroyed);

        public delegate void ServerBaseBrokenDelegate(ILogicObject areasGroup, List<ILogicObject> newAreaGroups);

        public delegate void ServerLandClaimsGroupChangedDelegate(
            ILogicObject area,
            [CanBeNull] ILogicObject areasGroupFrom,
            [CanBeNull] ILogicObject areasGroupTo);

        public delegate void ServerMergeDelegate(ILogicObject areasGroupFrom, ILogicObject areasGroupTo);

        public static event ServerLandClaimsGroupChangedDelegate ServerAreasGroupChanged;

        public static event Action<ILogicObject> ServerAreasGroupCreated;

        public static event Action<ILogicObject> ServerAreasGroupDestroyed;

        public static event ServerBaseBrokenDelegate ServerBaseBroken;

        public static event ServerMergeDelegate ServerBaseMerge;

        public static event DelegateServerObjectLandClaimDestroyed ServerObjectLandClaimDestroyed;

        public static event DelegateServerRaidBlockStartedOrExtended ServerRaidBlockStartedOrExtended;

        public static void ClientCannotInteractNotOwner(IStaticWorldObject worldObject)
        {
            CannotInteractMessageDisplay.ClientOnCannotInteract(worldObject,
                                                                ErrorNotLandOwner_Message,
                                                                isOutOfRange: false);
        }

        public static Task<LandClaimsGroupDecayInfo> ClientGetDecayInfoText(IStaticWorldObject landClaimWorldObject)
        {
            return Instance.CallServer(_ => _.ServerRemote_GetDecayInfo(landClaimWorldObject));
        }

        public static IEnumerable<ILogicObject> ClientGetKnownAreasForGroup(ILogicObject areasGroup)
        {
            foreach (var area in sharedLandClaimAreas)
            {
                if (ReferenceEquals(areasGroup,
                                    SharedGetLandClaimAreasGroup(area)))
                {
                    yield return area;
                }
            }
        }

        public static bool ClientIsOwnedArea(ILogicObject area, bool requireFactionPermission)
        {
            return ClientIsOwnedArea(area, requireFactionPermission, hasNoFactionPermission: out _);
        }

        public static bool ClientIsOwnedArea(
            ILogicObject area,
            bool requireFactionPermission,
            out bool hasNoFactionPermission)
        {
            hasNoFactionPermission = false;
            if (area is null)
            {
                throw new ArgumentNullException(nameof(area));
            }

            if (area.ProtoLogicObject is not LandClaimArea)
            {
                throw new Exception($"{area} is not a {nameof(LandClaimArea)}");
            }

            if (!area.ClientHasPrivateState)
            {
                return false;
            }

            if (requireFactionPermission
                && SharedGetAreaOwnerFactionClanTag(area) is { } factionClanTag
                && !string.IsNullOrEmpty(factionClanTag))
            {
                if (FactionSystem.ClientHasAccessRight(FactionMemberAccessRights.LandClaimManagement))
                {
                    return factionClanTag == FactionSystem.ClientCurrentFactionClanTag;
                }

                // has no faction or no access right
                // display "no permission" only if the base is owned by the player's faction
                hasNoFactionPermission = factionClanTag == FactionSystem.ClientCurrentFactionClanTag;
                return false;
            }

            return true;
        }

        public static async void ClientSetAreaOwners(ILogicObject area, List<string> newOwners)
        {
            var errorMessage = await Instance.CallServer(
                                   _ => Instance.ServerRemote_SetAreaOwners(area, newOwners));
            if (errorMessage is null)
            {
                return;
            }

            NotificationSystem.ClientShowNotification(
                title: null,
                message: errorMessage,
                color: NotificationColor.Bad);
        }

        public static double ServerAdjustDamageToUnclaimedBuilding(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damageMultiplier)
        {
            if (SharedIsObjectInsideAnyArea(targetObject))
            {
                // protected building
                return damageMultiplier;
            }

            // unprotected building
            if (weaponCache.ProtoExplosive is null)
            {
                // damage multiplier for weapon
                damageMultiplier *= 50;
                var protoTarget = targetObject.ProtoGameObject;
                if (protoTarget is IProtoObjectWall
                    || protoTarget is IProtoObjectDoor)
                {
                    // further increase the damage to unclaimed walls and doors to prevent their abuse
                    damageMultiplier *= 6;
                }
            }
            else
            {
                // damage multiplier for explosive
                damageMultiplier *= 25;
            }

            return damageMultiplier;
        }

        public static bool ServerCheckFutureBaseWillNotExceedSafeStorageCapacity(
            IProtoObjectLandClaim newProtoObjectLandClaim,
            in Vector2Ushort landClaimCenterTilePosition,
            ICharacter forCharacter)
        {
            if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
            {
                return true;
            }

            var maxSlotsCount = RatePvPSafeStorageCapacity.SharedValue;
            if (maxSlotsCount == 0)
            {
                return true;
            }

            var newAreaBounds = SharedCalculateLandClaimAreaBounds(
                landClaimCenterTilePosition,
                newProtoObjectLandClaim.LandClaimSize);

            using var tempListAreas = Api.Shared.GetTempList<ILogicObject>();
            using var tempListGroups = Api.Shared.GetTempList<ILogicObject>();
            SharedGatherAreasAndGroups(newAreaBounds, tempListAreas, tempListGroups);

            var totalOccupiedSlotsCount = 0;
            foreach (var areasGroup in tempListGroups.AsList())
            {
                totalOccupiedSlotsCount += LandClaimAreasGroup.GetPrivateState(areasGroup).ItemsContainerSafeStorage
                                                              .OccupiedSlotsCount;
            }

            // ensure that the total slots count is not exceeding the limit
            return totalOccupiedSlotsCount <= maxSlotsCount;
        }

        public static double ServerGetDecayDelayDurationForLandClaimAreas(
            List<ILogicObject> areas,
            bool isFounderDemoPlayer,
            out double normalDecayDelayDuration)
        {
            var decayDelayDuration = StructureConstants.DecayDelaySeconds;
            var decayDelayMultiplier
                = isFounderDemoPlayer
                      ? RateStructuresLandClaimDecayDelayDurationMultiplierForDemoPlayers.SharedValue
                      : RateStructuresLandClaimDecayDelayDurationMultiplier.SharedValue;
            decayDelayDuration *= decayDelayMultiplier;

            normalDecayDelayDuration = decayDelayDuration;
            foreach (var area in areas)
            {
                var worldObject = LandClaimArea.GetPrivateState(area)
                                               .ServerLandClaimWorldObject;

                var protoObjectLandClaim = (IProtoObjectLandClaim)worldObject.ProtoStaticWorldObject;
                normalDecayDelayDuration = Math.Max(protoObjectLandClaim.DecayDelayDuration.TotalSeconds
                                                    * decayDelayMultiplier,
                                                    normalDecayDelayDuration);
            }

            return isFounderDemoPlayer
                       ? decayDelayDuration
                       : normalDecayDelayDuration;
        }

        public static ILogicObject ServerGetLandClaimArea(IStaticWorldObject landClaimStructure)
        {
            return landClaimStructure.GetPublicState<ObjectLandClaimPublicState>()
                                     .LandClaimAreaObject;
        }

        public static bool ServerIsOwnedArea(ILogicObject area, ICharacter character, bool requireFactionPermission)
        {
            return ServerIsOwnedArea(area, character, requireFactionPermission, hasNoFactionPermission: out _);
        }

        /// <param name="requireFactionPermission">
        /// Is LandClaimManagement permission required
        /// in the case of the faction-owned area?
        /// </param>
        public static bool ServerIsOwnedArea(
            ILogicObject area,
            ICharacter character,
            bool requireFactionPermission,
            out bool hasNoFactionPermission)
        {
            hasNoFactionPermission = false;
            if (area is null)
            {
                Logger.Warning(nameof(ServerIsOwnedArea) + " - argument area is null");
                return false;
            }

            var privateState = LandClaimArea.GetPrivateState(area);
            if (!privateState.ServerGetLandOwners()
                             .Contains(character.Name))
            {
                return false;
            }

            if (requireFactionPermission
                && SharedGetAreaOwnerFactionClanTag(area) is { } factionClanTag
                && !string.IsNullOrEmpty(factionClanTag))
            {
                if (FactionSystem.ServerHasAccessRights(character,
                                                        FactionMemberAccessRights.LandClaimManagement,
                                                        out var characterFaction))
                {
                    return factionClanTag == FactionSystem.SharedGetClanTag(characterFaction);
                }

                // has no faction or no access right
                // display "no permission" only if the base is owned by the player's faction
                hasNoFactionPermission = factionClanTag == FactionSystem.SharedGetClanTag(characterFaction);
                return false;
            }

            return true;
        }

        public static void ServerNotifyCannotInteractNotOwner(ICharacter character, IStaticWorldObject worldObject)
        {
            Instance.CallClient(character, _ => _.ClientRemote_OnCannotInteractNotOwner(worldObject));
        }

        public static void ServerOnObjectLandClaimBuilt(
            ICharacter byCharacter,
            IStaticWorldObject landClaimStructure)
        {
            if (landClaimStructure?.ProtoStaticWorldObject
                    is not IProtoObjectLandClaim)
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
            areaPrivateState.DirectLandOwners = new NetworkSyncList<string>()
            {
                founderName
            };

            areaPublicState.SetupAreaProperties(areaPrivateState);

            // set this area to the structure public state
            landClaimStructure.GetPublicState<ObjectLandClaimPublicState>()
                              .LandClaimAreaObject = area;

            ServerEstablishAreasGroup(
                SharedGetLandClaimAreaBounds(area));

            ServerOnAddLandOwner(area, byCharacter, notify: false);
            Logger.Important("Land claim area added: " + area);
        }

        public static void ServerOnObjectLandClaimDestroyed(
            IStaticWorldObject landClaimStructure)
        {
            if (landClaimStructure?.ProtoStaticWorldObject
                    is not IProtoObjectLandClaim)
            {
                throw new Exception("Not a land claim structure: " + landClaimStructure);
            }

            var area = ServerGetLandClaimArea(landClaimStructure);
            if (area is null)
            {
                // area was already released (upgrade?)
                return;
            }

            var areaBounds = SharedGetLandClaimAreaBounds(area);

            StructureDecaySystem.ServerBeginDecayForStructuresInArea(areaBounds);
            var areaPrivateState = LandClaimArea.GetPrivateState(area);
            var areaPublicState = LandClaimArea.GetPublicState(area);
            var areasGroup = areaPublicState.LandClaimAreasGroup;
            var areasGroupPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            var areasGroupBounds = SharedGetLandClaimGroupBoundingArea(areasGroup);
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

            // is land claim structure is deconstructed by a crowbar?
            var isDeconstructed = ServerCharacters
                                  .EnumerateAllPlayerCharacters(onlyOnline: true)
                                  .Any(c => PlayerCharacter.GetPublicState(c).CurrentPublicActionState
                                                is DeconstructionActionState.PublicState deconstructionState
                                            && deconstructionState.TargetWorldObject == landClaimStructure);

            Api.SafeInvoke(() => ServerObjectLandClaimDestroyed?.Invoke(landClaimStructure,
                                                                        areaBounds,
                                                                        areaPublicState,
                                                                        areaPrivateState.IsDestroyedByPlayers,
                                                                        isDeconstructed));

            if (!oldAreasInGroup.AsList()
                                .SequenceEqual(areasGroupPrivateState.ServerLandClaimsAreas))
            {
                // areas group changed/broken to several areas
                var newGroups = oldAreasInGroup.AsList()
                                               .GroupBy(areas => LandClaimArea
                                                                 .GetPublicState(areas).LandClaimAreasGroup)
                                               .Select(g => g.Key)
                                               .ToList();

                newGroups.Remove(areasGroup);
                Api.SafeInvoke(() => ServerBaseBroken?.Invoke(areasGroup, newGroups));
            }
        }

        public static void ServerOnRaid(
            RectangleInt bounds,
            ICharacter byCharacter,
            bool isStructureDestroyed,
            bool forceEvenIfNoCharacter = false,
            double durationMultiplier = 1.0)
        {
            if (PveSystem.ServerIsPvE)
            {
                // no land claim raids on PvE
                return;
            }

            if (!forceEvenIfNoCharacter
                && byCharacter is null)
            {
                return;
            }

            if (byCharacter is not null
                && byCharacter.IsNpc)
            {
                // no raid block by NPC damage
                return;
            }

            if (!RaidingProtectionSystem.SharedCanRaid(bounds, showClientNotification: false)
                || !LandClaimShieldProtectionSystem.SharedCanActivateRaidblock(bounds, showClientNotification: false))
            {
                // raid block is not possible now
                return;
            }

            using var tempList = Api.Shared.GetTempList<ILogicObject>();
            SharedGetAreasInBounds(bounds, tempList, addGracePadding: false);

            foreach (var area in tempList.AsList())
            {
                if (byCharacter is not null
                    && ServerIsOwnedArea(area, byCharacter, requireFactionPermission: false))
                {
                    // don't start the raid timer if attack is performed by the owner of the area
                    continue;
                }

                ServerSetRaidblock(area, byCharacter, durationMultiplier, isStructureDestroyed);
            }
        }

        public static void ServerRegisterArea(ILogicObject area)
        {
            Api.Assert(area.ProtoLogicObject is LandClaimArea, "Wrong object type");
            sharedLandClaimAreas.Add(area);
            sharedLandClaimAreasCache = null;

            var areaPrivateState = area.GetPrivateState<LandClaimAreaPrivateState>();

            var areasGroup = SharedGetLandClaimAreasGroup(area);
            if (areasGroup is null)
            {
                // perhaps a new area
                return;
            }

            var faction = LandClaimAreasGroup.GetPublicState(areasGroup).ServerFaction;
            IEnumerable<string> owners;
            if (faction is null)
            {
                owners = areaPrivateState.DirectLandOwners;
            }
            else
            {
                owners = FactionSystem.ServerGetFactionMemberNames(faction);
                // faction-owned land claim area cannot have direct owners and founder 
                areaPrivateState.DirectLandOwners.Clear();
                areaPrivateState.LandClaimFounder = null;
            }

            if (owners is null)
            {
                return;
            }

            foreach (var owner in owners)
            {
                var player = ServerCharacters.GetPlayerCharacter(owner);
                if (player is null)
                {
                    continue;
                }

                ServerOnAddLandOwner(area, player, notify: player.ServerIsOnline);
            }
        }

        public static void ServerResetRaidblock(RectangleInt bounds)
        {
            using var tempList = Api.Shared.GetTempList<ILogicObject>();
            SharedGetAreasInBounds(bounds, tempList, addGracePadding: false);

            foreach (var area in tempList.AsList())
            {
                var areaPublicState = LandClaimArea.GetPublicState(area);
                var areasGroup = areaPublicState.LandClaimAreasGroup;
                var areasGroupPublicState = LandClaimAreasGroup.GetPublicState(areasGroup);
                areasGroupPublicState.LastRaidTime = null;
            }
        }

        public static void ServerSetRaidblock(
            ILogicObject area,
            [CanBeNull] ICharacter byCharacter,
            double durationMultiplier,
            bool isStructureDestroyed)
        {
            var areaPrivateState = LandClaimArea.GetPrivateState(area);
            var areaPublicState = LandClaimArea.GetPublicState(area);
            var areasGroup = areaPublicState.LandClaimAreasGroup;
            var areasGroupPublicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            var time = Server.Game.FrameTime;

            durationMultiplier = MathHelper.Clamp(durationMultiplier, 0, 1);
            if (durationMultiplier < 1.0)
            {
                time -= LandClaimSystemConstants.SharedRaidBlockDurationSeconds * (1 - durationMultiplier);
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (areasGroupPublicState.LastRaidTime >= time)
            {
                return;
            }

            var wasUnderRaidblock = time - areasGroupPublicState.LastRaidTime
                                    <= LandClaimSystemConstants.SharedRaidBlockDurationSeconds;

            areasGroupPublicState.LastRaidTime = time;
            Logger.Important(
                string.Format("Land claim area(s) being raided: {1}{0}Areas group: {2}",
                              Environment.NewLine,
                              areaPrivateState.ServerLandClaimWorldObject,
                              areasGroup));

            Api.SafeInvoke(
                () => ServerRaidBlockStartedOrExtended?.Invoke(area,
                                                               byCharacter,
                                                               isNewRaidBlock: !wasUnderRaidblock,
                                                               isStructureDestroyed: isStructureDestroyed));
        }

        public static void ServerUnregisterArea(ILogicObject area)
        {
            Api.Assert(area.ProtoLogicObject is LandClaimArea, "Wrong object type");
            sharedLandClaimAreas.Remove(area);
            sharedLandClaimAreasCache = null;

            var owners = area.GetPrivateState<LandClaimAreaPrivateState>().ServerGetLandOwners();
            if (owners is null)
            {
                return;
            }

            foreach (var owner in owners)
            {
                var player = ServerCharacters.GetPlayerCharacter(owner);
                if (player is null)
                {
                    continue;
                }

                SharedGetPlayerOwnedAreas(player).Remove(area);
                if (player.ServerIsOnline)
                {
                    ServerWorld.ExitPrivateScope(player, area);
                }
            }
        }

        public static IStaticWorldObject ServerUpgrade(
            IStaticWorldObject oldStructure,
            IProtoObjectStructure protoStructureUpgrade,
            ICharacter character)
        {
            if (oldStructure?.ProtoStaticWorldObject is not IProtoObjectLandClaim)
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
            var upgradedObject = ServerWorld.CreateStaticWorldObject(protoStructureUpgrade, tilePosition);

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
                SharedGetLandClaimAreaBounds(area));

            // Even though the area's group didn't change,
            // we need to notify power system so it will rebuild power grid for the area's group.
            Api.SafeInvoke(() => ServerAreasGroupChanged?.Invoke(area,
                                                                 null,
                                                                 areaPublicState.LandClaimAreasGroup));

            return upgradedObject;
        }

        public static RectangleInt SharedCalculateLandClaimAreaBounds(Vector2Ushort centerTilePosition, ushort size)
        {
            var pos = centerTilePosition;
            var halfSize = size / 2;

            var start = new Vector2Ushort(
                (ushort)Math.Max(pos.X - halfSize, 0),
                (ushort)Math.Max(pos.Y - halfSize, 0));

            var endX = Math.Min(pos.X + halfSize, ushort.MaxValue);
            var endY = Math.Min(pos.Y + halfSize, ushort.MaxValue);

            var calculatedSize = new Vector2Ushort((ushort)(endX - start.X),
                                                   (ushort)(endY - start.Y));
            return new RectangleInt(start, size: calculatedSize);
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

        public static bool SharedCheckCanDeconstruct(
            IStaticWorldObject worldObject,
            ICharacter character,
            out bool hasNoFactionPermission)
        {
            hasNoFactionPermission = false;
            // Please note: the game already have validated that the target object is a structure
            var protoStructure = (IProtoObjectStructure)worldObject.ProtoGameObject;
            if (protoStructure is ObjectWallDestroyed)
            {
                // always allow deconstruct a destroyed wall object even if it's in another player's land claim
                return true;
            }

            if (!protoStructure.SharedCanDeconstruct(worldObject, character))
            {
                hasNoFactionPermission = false;
                return false;
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
            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForBounds(worldObjectBounds))
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (!areaBounds.IntersectsLoose(worldObjectBounds))
                {
                    continue;
                }

                isThereAnyArea = true;
                // intersection with area found - check if player owns the area
                if (SharedIsOwnedArea(area,
                                      character,
                                      requireFactionPermission: true,
                                      out var localHasNoFactionPermission))
                {
                    // player owns the land claim area
                    return true;
                }

                if (localHasNoFactionPermission)
                {
                    hasNoFactionPermission = true;
                }
            }

            if (isThereAnyArea)
            {
                // found a not owned area
                return false;
            }

            // no area found
            if (protoStructure is ProtoObjectConstructionSite)
            {
                // can deconstruct blueprints if there is no land claim area
                return true;
            }

            // let's check whether there is a grace area of the land claim area owned by the player
            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForBounds(worldObjectBounds))
            {
                var areaBoundsWithPadding = SharedGetLandClaimAreaBounds(area, addGracePadding: true);
                if (!areaBoundsWithPadding.IntersectsLoose(worldObjectBounds))
                {
                    continue;
                }

                // intersection with grace area found - check if player owns the area
                if (SharedIsOwnedArea(area, character, requireFactionPermission: true))
                {
                    // player owns the land claim area containing this grace area
                    return true;
                }
            }

            return false;
        }

        // need to verify that area bounds will not intersect with the any existing areas (except from the same founder)
        public static bool SharedCheckCanPlaceOrUpgradeLandClaimThere(
            IProtoObjectLandClaim newProtoObjectLandClaim,
            Vector2Ushort landClaimCenterTilePosition,
            ICharacter forCharacter,
            out bool hasNoFactionPermission)
        {
            if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
            {
                hasNoFactionPermission = false;
                return true;
            }

            var newAreaBounds = SharedCalculateLandClaimAreaBounds(
                landClaimCenterTilePosition,
                (ushort)(newProtoObjectLandClaim.LandClaimWithGraceAreaSize
                         // reduce the outer bounds as it's the buffer area
                         - MinPaddingSizeOneDirection * 2));

            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForBounds(newAreaBounds))
            {
                var areaBoundsWithPadding = SharedGetLandClaimAreaBounds(area, addGracePadding: true);
                if (!areaBoundsWithPadding.IntersectsLoose(newAreaBounds))
                {
                    // there is no area (even with the padding/grace area)
                    continue;
                }

                if (!SharedIsOwnedArea(area,
                                       forCharacter,
                                       requireFactionPermission: true,
                                       out hasNoFactionPermission))
                {
                    // the grace/padding area of another player's land claim owner
                    // or has no permission to this faction-owned area
                    return false;
                }
            }

            hasNoFactionPermission = false;
            return true;
        }

        public static bool SharedCheckCanPlaceOrUpgradeLandClaimThereConsideringShieldProtection(
            IProtoObjectLandClaim newProtoObjectLandClaim,
            Vector2Ushort landClaimCenterTilePosition,
            ICharacter forCharacter)
        {
            var newAreaBounds = SharedCalculateLandClaimAreaBounds(
                landClaimCenterTilePosition,
                (ushort)(newProtoObjectLandClaim.LandClaimSize + 2));

            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForBounds(newAreaBounds))
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area, addGracePadding: false);
                if (!areaBounds.IntersectsLoose(newAreaBounds))
                {
                    // there is no area (even with the padding/grace area)
                    continue;
                }

                if (!SharedIsOwnedArea(area, forCharacter, requireFactionPermission: true))
                {
                    // the area is not owned - it will be checked by another validator
                    continue;
                }

                var areasGroup = SharedGetLandClaimAreasGroup(area);
                if (LandClaimShieldProtectionSystem.SharedGetShieldPublicStatus(areasGroup)
                    == ShieldProtectionStatus.Active)
                {
                    // intersecting with a land claim area that is under active shield
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// We're assuming the necessary checks for permissions and faction ownership have been already made.
        /// This method with determine which new land claims will be included into the faction ownership
        /// during building or upgrading a land claim.
        /// The method returns true if this number doesn't exceed the available faction claim number limit.
        /// </summary>
        public static bool SharedCheckFutureBaseWillNotExceedFactionLandClaimLimit(
            IProtoObjectLandClaim newProtoObjectLandClaim,
            in Vector2Ushort landClaimCenterTilePosition,
            ICharacter forCharacter,
            bool isForNewLandClaim)
        {
            if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
            {
                return true;
            }

            var faction = Api.IsServer
                              ? FactionSystem.ServerGetFaction(forCharacter)
                              : FactionSystem.ClientCurrentFaction; // client knows only about its current faction
            if (faction is null)
            {
                return true;
            }

            var factionClanTag = FactionSystem.SharedGetClanTag(faction);
            var factionOwnedAreas = SharedEnumerateAllFactionAreas(factionClanTag).ToList();

            var claimLimitRemains = FactionConstants.SharedGetFactionLandClaimsLimit(
                                        Faction.GetPublicState(faction).Level)
                                    - factionOwnedAreas.Count;

            if (!isForNewLandClaim
                && claimLimitRemains < 0)
            {
                // limit already reached
                return false;
            }

            var newAreaBounds = SharedCalculateLandClaimAreaBounds(
                landClaimCenterTilePosition,
                newProtoObjectLandClaim.LandClaimSize);

            using var tempListAreas = Api.Shared.GetTempList<ILogicObject>();
            using var tempListGroups = Api.Shared.GetTempList<ILogicObject>();
            SharedGatherAreasAndGroups(newAreaBounds, tempListAreas, tempListGroups);

            if (tempListAreas.AsList()
                             .All(a => string.IsNullOrEmpty(SharedGetAreaOwnerFactionClanTag(a))))

            {
                // there are no areas nearby owned by the faction
                // the new or upgraded claim will not increase the number of faction owned claims
                return true;
            }

            if (isForNewLandClaim)
            {
                // new claim will reduce the limit by one as it will become a new faction claim!
                claimLimitRemains--;
            }

            foreach (var area in factionOwnedAreas)
            {
                // these areas are already counted in the limit 
                tempListAreas.Remove(area);
            }

            // ensure that the total slots count is not exceeding the limit
            /*Logger.Dev("New areas to include found nearby: "
                       + tempListAreas.Count
                       + " faction claims limit (remains): "
                       + claimLimitRemains);*/
            return tempListAreas.Count <= claimLimitRemains;
        }

        // need to verify that area bounds will not intersect with the any existing areas of demo players (even for the same founder if it's a demo account)
        public static bool SharedCheckNoLandClaimByDemoPlayers(
            IProtoObjectLandClaim newProtoObjectLandClaim,
            Vector2Ushort landClaimCenterTilePosition,
            ICharacter forCharacter,
            ILogicObject exceptAreasGroup)
        {
            if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
            {
                return true;
            }

            var newAreaBounds = SharedCalculateLandClaimAreaBounds(
                landClaimCenterTilePosition,
                (ushort)(newProtoObjectLandClaim.LandClaimWithGraceAreaSize
                         // reduce the outer bounds as it's the buffer area
                         - MinPaddingSizeOneDirection * 2));

            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForBounds(newAreaBounds))
            {
                var areaBoundsWithPadding = SharedGetLandClaimAreaBounds(area, addGracePadding: true);
                if (!areaBoundsWithPadding.IntersectsLoose(newAreaBounds))
                {
                    // there is no area (even with the padding/grace area)
                    continue;
                }

                var areasGroup = SharedGetLandClaimAreasGroup(area);
                if (ReferenceEquals(exceptAreasGroup, areasGroup))
                {
                    // this check is required to allow demo players upgrade land claims on their base
                    continue;
                }

                if (IsServer && forCharacter.ServerIsDemoVersion
                    || IsClient && Api.Client.MasterServer.IsDemoVersion)
                {
                    // the player is a demo player, restrict construction
                    return false;
                }

                var areasGroupPublicState = LandClaimAreasGroup.GetPublicState(areasGroup);
                if (areasGroupPublicState.IsFounderDemoPlayer)
                {
                    // the grace/padding area of another player's land claim owner who is a demo player
                    return false;
                }
            }

            return true;
        }

        public static IReadOnlyList<ILogicObject> SharedEnumerateAllAreas()
        {
            return (IReadOnlyList<ILogicObject>)sharedLandClaimAreas;
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

            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForBounds(bounds))
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (addGracePadding)
                {
                    var protoObjectLandClaim = LandClaimArea.GetPublicState(area).ProtoObjectLandClaim;
                    areaBounds = areaBounds.Inflate(protoObjectLandClaim.LandClaimGraceAreaPaddingSizeOneDirection);
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
            var protoObjectLandClaim = publicState.ProtoObjectLandClaim;
            var size = protoObjectLandClaim.LandClaimSize;
            if (addGracePadding)
            {
                size = (ushort)(size + 2 * protoObjectLandClaim.LandClaimGraceAreaPaddingSizeOneDirection);
            }

            return SharedCalculateLandClaimAreaBounds(publicState.LandClaimCenterTilePosition, size);
        }

        public static ILogicObject SharedGetLandClaimAreasGroup(
            Vector2Ushort tilePosition,
            bool addGracePadding = false)
        {
            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForPosition(tilePosition))
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

        public static ILogicObject SharedGetLandClaimAreasGroup(
            RectangleInt bounds,
            bool addGracePadding = false)
        {
            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForBounds(bounds))
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area, addGracePadding);
                if (!areaBounds.IntersectsLoose(bounds))
                {
                    continue;
                }

                var areaPublicState = LandClaimArea.GetPublicState(area);
                var areasGroup = areaPublicState.LandClaimAreasGroup;
                return areasGroup;
            }

            return null;
        }

        public static ILogicObject SharedGetLandClaimAreasGroup(ILogicObject area)
        {
            return LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
        }

        public static ILogicObject SharedGetLandClaimAreasGroup(IStaticWorldObject worldObject)
        {
            return SharedGetLandClaimAreasGroup(worldObject.Bounds);
        }

        public static RectangleInt SharedGetLandClaimGroupBoundingArea(
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
                if (!ReferenceEquals(otherGroup, group))
                {
                    continue;
                }

                hasBounds = true;
                var bounds = SharedGetLandClaimAreaBounds(area);
                if (addGraceAreaPadding)
                {
                    bounds = bounds.Inflate(areaPublicState.ProtoObjectLandClaim
                                                           .LandClaimGraceAreaPaddingSizeOneDirection);
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

        public static Vector2Ushort SharedGetLandClaimGroupCenterPosition(ILogicObject areasGroup)
        {
            var bounds = SharedGetLandClaimGroupBoundingArea(areasGroup);
            return ((ushort)(bounds.X + bounds.Width / 2.0),
                    (ushort)(bounds.Y + bounds.Height / 2.0));
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
            return timeSinceRaidStart < LandClaimSystemConstants.SharedRaidBlockDurationSeconds;
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

        public static bool SharedIsLandClaimedByAnyone(in Vector2Ushort tilePosition)
        {
            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForPosition(tilePosition))
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
            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForBounds(bounds))
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (!areaBounds.IntersectsLoose(bounds))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public static bool SharedIsObjectInsideAnyArea(IStaticWorldObject worldObject)
        {
            var bounds = worldObject.Bounds;
            if (!SharedIsLandClaimedByAnyone(bounds))
            {
                return false;
            }

            // verify full coverage of all object tiles by any land claim area
            if (bounds.Width == 1
                && bounds.Height == 1)
            {
                return true;
            }

            using var tempListTilePositionsRemains = Api.Shared.GetTempList<Vector2Ushort>();
            var tilePositionsRemains = tempListTilePositionsRemains.AsList();
            foreach (var occupiedTilePosition in worldObject.OccupiedTilePositions)
            {
                tilePositionsRemains.Add(occupiedTilePosition);
            }

            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForBounds(bounds))
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (!areaBounds.IntersectsLoose(bounds))
                {
                    continue;
                }

                for (var index = 0; index < tilePositionsRemains.Count; index++)
                {
                    var tilePosition = tilePositionsRemains[index];
                    if (!areaBounds.Contains(tilePosition))
                    {
                        continue;
                    }

                    tilePositionsRemains.RemoveAt(index--);
                    if (tilePositionsRemains.Count == 0)
                    {
                        // the bounds are fully covered by this areas group
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool SharedIsObjectInsideOwnedOrFreeArea(
            IStaticWorldObject worldObject,
            ICharacter who,
            bool requireFactionPermission)
        {
            return SharedIsObjectInsideOwnedOrFreeArea(worldObject, who, requireFactionPermission, out _);
        }

        public static bool SharedIsObjectInsideOwnedOrFreeArea(
            IStaticWorldObject worldObject,
            ICharacter who,
            bool requireFactionPermission,
            out bool hasNoFactionPermission)
        {
            hasNoFactionPermission = false;
            var foundAnyAreas = false;
            var startTilePosition = worldObject.TilePosition;
            var worldObjectLayoutTileOffsets = worldObject.ProtoStaticWorldObject.Layout.TileOffsets;

            var areasCache = SharedGetLandClaimAreasCache();
            foreach (var tileOffset in worldObjectLayoutTileOffsets)
            {
                var tilePosition = startTilePosition.AddAndClamp(tileOffset);
                foreach (var area in areasCache.EnumerateForPosition(tilePosition))
                {
                    var areaBounds = SharedGetLandClaimAreaBounds(area);
                    if (!areaBounds.Contains(tilePosition))
                    {
                        // the object is not inside this area
                        continue;
                    }

                    if (SharedIsOwnedArea(area, who, requireFactionPermission, out var localHasNoFactionPermission))
                    {
                        // player owns this area
                        return true;
                    }

                    if (localHasNoFactionPermission)
                    {
                        hasNoFactionPermission = true;
                    }

                    foundAnyAreas = true;
                }
            }

            // return true only if there are no areas (free land)
            return !foundAnyAreas;
        }

        public static bool SharedIsOwnedArea(
            ILogicObject area,
            ICharacter forCharacter,
            bool requireFactionPermission,
            out bool hasNoFactionPermission)
        {
            return IsServer
                       ? ServerIsOwnedArea(area, forCharacter, requireFactionPermission, out hasNoFactionPermission)
                       : ClientIsOwnedArea(area, requireFactionPermission, out hasNoFactionPermission);
        }

        public static bool SharedIsOwnedArea(ILogicObject area, ICharacter forCharacter, bool requireFactionPermission)
        {
            return SharedIsOwnedArea(area,
                                     forCharacter,
                                     requireFactionPermission,
                                     hasNoFactionPermission: out _);
        }

        public static bool SharedIsOwnedLand(
            Vector2Ushort tilePosition,
            ICharacter who,
            bool requireFactionPermission,
            out ILogicObject ownedArea)
        {
            return SharedIsOwnedLand(tilePosition,
                                     who,
                                     requireFactionPermission,
                                     hasNoFactionPermission: out _,
                                     out ownedArea);
        }

        public static bool SharedIsOwnedLand(
            Vector2Ushort tilePosition,
            ICharacter who,
            bool requireFactionPermission,
            out bool hasNoFactionPermission,
            out ILogicObject ownedArea)
        {
            hasNoFactionPermission = false;
            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForPosition(tilePosition))
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area);
                if (!areaBounds.Contains(tilePosition))
                {
                    // the object is not inside this area
                    continue;
                }

                if (SharedIsOwnedArea(area, who, requireFactionPermission, out var localHasNoFactionPermission))
                {
                    // player owns this area
                    ownedArea = area;
                    return true;
                }

                if (localHasNoFactionPermission)
                {
                    hasNoFactionPermission = true;
                }
            }

            // there are no areas or player don't own any area there
            ownedArea = null;
            return false;
        }

        public static bool SharedIsPositionInsideOwnedOrFreeArea(
            Vector2Ushort tilePosition,
            ICharacter who,
            bool requireFactionPermission,
            bool addGracePaddingWithoutBuffer = false)
        {
            var foundAnyAreas = false;
            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForPosition(tilePosition))
            {
                var areaBounds = SharedGetLandClaimAreaBounds(area, addGracePaddingWithoutBuffer);
                if (addGracePaddingWithoutBuffer)
                {
                    // remove the border cases (touching with the land claim grace area and buffer)
                    areaBounds = areaBounds.Inflate(-1 - MinPaddingSizeOneDirection);
                }

                if (!areaBounds.Contains(tilePosition))
                {
                    // the tile position is not inside this area
                    continue;
                }

                if (SharedIsOwnedArea(area, who, requireFactionPermission))
                {
                    // player owns this area
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

                if (validatorNoRaid.CheckFunction(
                    new ConstructionTileRequirements.Context(
                        occupiedTile,
                        character,
                        protoStaticWorldObject,
                        tileOffset,
                        startTilePosition)))
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
                Instance.ClientRemote_ShowNotificationActionForbiddenUnderRaidblock();
            }
            else
            {
                Instance.CallClient(character,
                                    _ => _.ClientRemote_ShowNotificationActionForbiddenUnderRaidblock());
            }
        }

        public static bool ValidateIsNotUnderRaidblock(
            IStaticWorldObject worldObject,
            ICharacter forCharacter,
            bool showNotification = true)
        {
            if (!SharedIsUnderRaidBlock(forCharacter, worldObject))
            {
                return true;
            }

            if (showNotification)
            {
                SharedSendNotificationActionForbiddenUnderRaidblock(forCharacter);
            }

            return false;
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            ServerCharacters.PlayerNameChanged += ServerPlayerNameChangedHandler;
            ServerCharacters.PlayerOnlineStateChanged += ServerPlayerOnlineStateChangedHandler;
        }

        /// <summary>
        /// Checks the area land claims, release those that do not belong to the group anymore.
        /// Destroys group if it's empty.
        /// </summary>
        private static void ServerCleanupAreasGroup(ILogicObject areasGroup)
        {
            if (areasGroup is null
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
                    && areasGroup == SharedGetLandClaimAreasGroup(area))
                {
                    continue;
                }

                // this area now doesn't belong to the areas group
                areas.RemoveAt(index);
                index--;
                Logger.Important($"Land claim areas group {areasGroup} - released area: {area}");
                tempRemovedAreas.Add(area);
                Api.SafeInvoke(() => ServerAreasGroupChanged?.Invoke(area, areasGroup, null));
            }

            if (areas.Count > 0)
            {
                return;
            }

            // areas group has no areas which is using it
            Api.SafeInvoke(() => ServerAreasGroupDestroyed?.Invoke(areasGroup));
            Logger.Important("Destroying land claim areas group - not used anymore: " + areasGroup);
            Api.Server.World.DestroyObject(areasGroup);

            foreach (var area in tempRemovedAreas.AsList())
            {
                if (area.IsDestroyed)
                {
                    continue;
                }

                var otherAreasGroup = SharedGetLandClaimAreasGroup(area);
                if (otherAreasGroup is null)
                {
                    continue;
                }

                Api.SafeInvoke(() => ServerBaseMerge?.Invoke(areasGroup, otherAreasGroup));
                LandClaimAreasGroup.ServerOnBaseMerged(areasGroup, otherAreasGroup);
                // that's correct! we return here as the base might be merged only with a single other base
                return;
            }
        }

        private static void ServerDropItems(IStaticWorldObject landClaimStructure, ILogicObject areasGroup)
        {
            // try drop items from the safe storage
            var itemsContainer = LandClaimAreasGroup.GetPrivateState(areasGroup).ItemsContainerSafeStorage;
            ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                landClaimStructure.OccupiedTile,
                itemsContainer,
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
            // For a candidate for the group let's take the currently existing group which is owned by a faction.
            var currentGroup = tempListGroups
                               .AsList()
                               .FirstOrDefault(g => !string.IsNullOrEmpty(LandClaimAreasGroup
                                                                          .GetPublicState(g).FactionClanTag));
            if (currentGroup is null)
            {
                // There are no groups owned by a faction.
                // For a candidate for the group let's take the currently existing group
                // // with the max number of the land claims OR which is owned by a faction.
                currentGroup = tempListGroups.AsList()
                                             .MaximumOrDefault(
                                                 g => LandClaimAreasGroup
                                                      .GetPrivateState(g).ServerLandClaimsAreas.Count);
            }

            if (currentGroup is null)
            {
                // need to create a new group
                currentGroup = Api.Server.World.CreateLogicObject<LandClaimAreasGroup>();
                Logger.Important("Created land claim areas group: " + currentGroup);

                Api.SafeInvoke(() => ServerAreasGroupCreated?.Invoke(currentGroup));
            }

            var groupPrivateState = LandClaimAreasGroup.GetPrivateState(currentGroup);
            groupPrivateState.ServerLandClaimsAreas ??= new List<ILogicObject>();

            var areasGroupAreasList = groupPrivateState.ServerLandClaimsAreas;

            // assign the group to all the land claim areas there
            foreach (var area in tempListAreas.AsList())
            {
                var areaPublicState = LandClaimArea.GetPublicState(area);
                var areasGroup = areaPublicState.LandClaimAreasGroup;
                if (areasGroup == currentGroup)
                {
                    continue;
                }

                ServerUnregisterArea(area);

                areaPublicState.LandClaimAreasGroup = currentGroup;
                Logger.Important($"Assigned land claim areas group: {currentGroup} - to {area}");
                areasGroupAreasList.Add(area);

                if (areasGroup is not null)
                {
                    // group changed - copy necessary properties such as last raid time
                    LandClaimAreasGroup.ServerOnGroupChanged(area, areasGroup, currentGroup);
                }

                ServerRegisterArea(area);
                Api.SafeInvoke(() => ServerAreasGroupChanged?.Invoke(area, areasGroup, currentGroup));
            }

            foreach (var otherGroup in tempListGroups.AsList())
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
                ServerTimersSystem.AddAction(delaySeconds: 0.1,
                                             OnAddLandOwner);
            }
            else
            {
                OnAddLandOwner();
            }

            void OnAddLandOwner()
            {
                if (!ServerIsOwnedArea(area, playerToAdd, requireFactionPermission: false))
                {
                    // The owner is already removed during the short delay? That's was quick!
                    return;
                }

                ServerWorld.EnterPrivateScope(playerToAdd, area);
                var ownedAreas = SharedGetPlayerOwnedAreas(playerToAdd);
                if (ownedAreas.Contains(area))
                {
                    return;
                }

                ownedAreas.Add(area);

                if (notify
                    && playerToAdd.ServerIsOnline)
                {
                    Instance.CallClient(
                        playerToAdd,
                        _ => _.ClientRemote_OnLandOwnerStateChanged(area));
                }
            }
        }

        private static void ServerOnRemoveLandOwner(ILogicObject area, ICharacter removedPlayer)
        {
            InteractableWorldObjectHelper.ServerTryAbortInteraction(
                removedPlayer,
                LandClaimArea.GetPrivateState(area).ServerLandClaimWorldObject);

            ServerWorld.ExitPrivateScope(removedPlayer, area);
            var isOwnedAreaRemoved = SharedGetPlayerOwnedAreas(removedPlayer).Remove(area);
            if (!isOwnedAreaRemoved)
            {
                return;
            }

            if (removedPlayer.ServerIsOnline)
            {
                Instance.CallClient(removedPlayer,
                                    _ => _.ClientRemote_OnLandOwnerStateChanged(area));
            }
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
                var owners = privateState.DirectLandOwners;
                if (owners is null)
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

        private static void ServerPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
        {
            if (!isOnline)
            {
                return;
            }

            // add to the character private scope all owned areas
            foreach (var area in SharedGetPlayerOwnedAreas(character))
            {
                ServerWorld.EnterPrivateScope(character, area);
            }
        }

        private static void ServerTryRebuildLandClaimsGroups(RectangleInt bounds)
        {
            using var tempListAreas = Api.Shared.GetTempList<ILogicObject>();
            using var tempListGroups = Api.Shared.GetTempList<ILogicObject>();
            SharedGatherAreasAndGroups(bounds, tempListAreas, tempListGroups);

            foreach (var areasGroup in tempListGroups.AsList())
            {
                ServerReleaseGroupUnconnectedAreas(areasGroup);
            }

            // rebuild groups for every land claim without a group there
            foreach (var otherArea in tempListAreas.AsList())
            {
                if (SharedGetLandClaimAreasGroup(otherArea) is null)
                {
                    ServerEstablishAreasGroup(
                        SharedGetLandClaimAreaBounds(otherArea));
                }
            }

            void ServerReleaseGroupUnconnectedAreas(ILogicObject areasGroup)
            {
                ServerCleanupAreasGroup(areasGroup);

                var currentAreas = LandClaimAreasGroup.GetPrivateState(areasGroup).ServerLandClaimsAreas;
                using var tempListCurrentGroupAreas = Api.Shared.GetTempList<ILogicObject>();
                SharedGetConnectedAreas(tempListCurrentGroupAreas.AsList(),
                                        SharedGetLandClaimAreaBounds(currentAreas[0]));

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
            ITempList<ILogicObject> tempListGroups,
            bool addGracePaddingToEachArea = false)
        {
            SharedGetConnectedAreas(tempListAreas.AsList(),
                                    bounds,
                                    addGracePaddingToEachArea);

            // gather all groups for the areas found there
            foreach (var area in tempListAreas.AsList())
            {
                var areasGroup = SharedGetLandClaimAreasGroup(area);
                if (areasGroup is not null)
                {
                    tempListGroups.AddIfNotContains(areasGroup);
                }
            }
        }

        private static void SharedGetConnectedAreas(
            List<ILogicObject> result,
            RectangleInt bounds,
            bool addGracePadding = false)
        {
            // TODO: this enumeration might be slow
            // find areas in bounds
            foreach (var area in sharedLandClaimAreas)
            {
                if (area.IsDestroyed)
                {
                    continue;
                }

                var areaBounds = SharedGetLandClaimAreaBounds(area,
                                                              addGracePadding: addGracePadding);
                if (addGracePadding
                        ? areaBounds.IntersectsLoose(bounds)
                        : areaBounds.Intersects(bounds))
                {
                    result.Add(area);
                }
            }

            // collect adjacent areas
            for (var index = 0; index < result.Count; index++)
            {
                var currentArea = result[index];
                var currentAreaBounds = SharedGetLandClaimAreaBounds(currentArea,
                                                                     addGracePadding: addGracePadding);

                if (addGracePadding)
                {
                    // deflate by 1 tile as bases have 1 tile buffer area around them that allows intersection
                    currentAreaBounds = currentAreaBounds.Inflate(-1);
                }

                // find adjacent areas
                foreach (var otherArea in sharedLandClaimAreas)
                {
                    var otherAreaBounds = SharedGetLandClaimAreaBounds(otherArea,
                                                                       addGracePadding: addGracePadding);
                    if ((addGracePadding
                             ? otherAreaBounds.IntersectsLoose(currentAreaBounds)
                             : otherAreaBounds.Intersects(currentAreaBounds))
                        && !result.Contains(otherArea))
                    {
                        // found new area - it will be also checked for adjacent neighbors
                        result.Add(otherArea);
                    }
                }
            }
        }

        private static LandClaimAreasCache SharedGetLandClaimAreasCache()
        {
            if (sharedLandClaimAreasCache is not null)
            {
                return sharedLandClaimAreasCache;
            }

            var stopwatch = Stopwatch.StartNew();
            sharedLandClaimAreasCache = new LandClaimAreasCache(sharedLandClaimAreas);
            Logger.Important(
                $"Land claim areas cache rebuilt: spent {stopwatch.Elapsed.TotalMilliseconds:0.##} ms, contains {sharedLandClaimAreas.Count} land claims");

            return sharedLandClaimAreasCache;
        }

        private static int SharedGetMaxBaseSizeTiles()
        {
            // 2 tiles - represents an extra 1 tile padding around every side of the base with all T5 land claims
            return 2 + MaxLandClaimSize.Value * MaxNumberOfLandClaimsInRow;
        }

        private static void SharedGetPersonalLandClaimsNumberLimit(
            ICharacter character,
            out int currentNumber,
            out int maxNumber)
        {
            // calculate max number of land claims
            var finalStatsCache = character.SharedGetFinalStatsCache();
            maxNumber = (int)Math.Round(finalStatsCache[StatName.LandClaimsMaxNumber],
                                        MidpointRounding.AwayFromZero);

            // calculate current number of land claims
            currentNumber = 0;
            foreach (var area in sharedLandClaimAreas)
            {
                if (IsClient && !area.ClientHasPrivateState)
                {
                    continue;
                }

                var privateState = LandClaimArea.GetPrivateState(area);
                if (privateState.LandClaimFounder == character.Name)
                {
                    currentNumber++;
                }
            }
        }

        private static NetworkSyncList<ILogicObject> SharedGetPlayerOwnedAreas(ICharacter player)
        {
            return PlayerCharacter.GetPrivateState(player)
                                  .OwnedLandClaimAreas;
        }

        private static bool ValidatorIsOwnedLandCheck(
            ConstructionTileRequirements.Context context,
            bool requireFactionPermission,
            out bool hasNoFactionPermission)
        {
            var forCharacter = context.CharacterBuilder;
            if (forCharacter is null)
            {
                hasNoFactionPermission = false;
                return true;
            }

            if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
            {
                hasNoFactionPermission = false;
                return true;
            }

            return SharedIsOwnedLand(context.Tile.Position,
                                     forCharacter,
                                     requireFactionPermission: requireFactionPermission,
                                     hasNoFactionPermission: out hasNoFactionPermission,
                                     out _);
        }

        private static bool ValidatorIsOwnedOrFreeLandCheck(
            ConstructionTileRequirements.Context context,
            bool requireFactionPermission,
            out bool hasNoFactionPermission)
        {
            hasNoFactionPermission = false;

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

            var noAreasThere = true;
            foreach (var area in SharedGetLandClaimAreasCache().EnumerateForPosition(position))
            {
                var areaBoundsDirect = SharedGetLandClaimAreaBounds(area);
                var areaBoundsWithPadding = areaBoundsDirect.Inflate(LandClaimArea.GetPublicState(area)
                                                                         .ProtoObjectLandClaim
                                                                         .LandClaimGraceAreaPaddingSizeOneDirection);

                if (!areaBoundsWithPadding.Contains(position))
                {
                    // there is no area (even with the padding/grace area)
                    continue;
                }

                if (areaBoundsDirect.Contains(position))
                {
                    // there is a direct area (not padding/grace area)
                    if (SharedIsOwnedArea(area,
                                          forCharacter,
                                          requireFactionPermission,
                                          out var localHasNoFactionPermission))
                    {
                        // player owns the area
                        hasNoFactionPermission = false;
                        return true;
                    }

                    if (localHasNoFactionPermission)
                    {
                        hasNoFactionPermission = true;
                    }

                    noAreasThere = false;
                }
                else
                {
                    // there is no direct area (only the padding/grace area)
                    if (!SharedIsOwnedArea(area,
                                           forCharacter,
                                           requireFactionPermission,
                                           out var localHasNoFactionPermission))
                    {
                        // the grace/padding area of another player's land claim found
                        noAreasThere = false;

                        if (localHasNoFactionPermission)
                        {
                            hasNoFactionPermission = true;
                        }
                    }
                }
            }

            // if we've come to here here and there are any areas it means the player doesn't own any of them
            return noAreasThere;
        }

        private static bool ValidatorNewLandClaimNoLandClaimIntersectionsCheck(
            ConstructionTileRequirements.Context context,
            out bool hasNoFactionPermission)
        {
            hasNoFactionPermission = false;
            var forCharacter = context.CharacterBuilder;
            if (forCharacter is null)
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
            return SharedCheckCanPlaceOrUpgradeLandClaimThere(
                protoObjectLandClaim,
                centerTilePosition,
                forCharacter,
                hasNoFactionPermission: out hasNoFactionPermission);
        }

        private void ClientRemote_OnCannotInteractNotOwner(IStaticWorldObject worldObject)
        {
            ClientCannotInteractNotOwner(worldObject);
        }

        private void ClientRemote_OnLandClaimUpgraded(ILogicObject area)
        {
            ClientLandClaimAreaManager.OnLandClaimUpgraded(area);
        }

        private void ClientRemote_OnLandOwnerStateChanged(ILogicObject area)
        {
            Logger.Important("Land claim area ownership changed: " + area);
            ClientLandClaimAreaManager.OnLandOwnerStateChanged(area);
        }

        private void ClientRemote_ShowNotificationActionForbiddenUnderRaidblock()
        {
            NotificationSystem.ClientShowNotification(
                CoreStrings.Notification_ActionForbidden,
                ErrorRaidBlockActionRestricted_Message,
                color: NotificationColor.Bad);
        }

        [RemoteCallSettings(timeInterval: 1)]
        private LandClaimsGroupDecayInfo ServerRemote_GetDecayInfo(IStaticWorldObject landClaimStructure)
        {
            var character = ServerRemoteContext.Character;
            if (!Server.World.IsInPrivateScope(landClaimStructure, character))
            {
                throw new Exception("Cannot interact with the land claim object is not in private scope: "
                                    + landClaimStructure);
            }

            var area = ServerGetLandClaimArea(landClaimStructure);
            var areasGroup = SharedGetLandClaimAreasGroup(area);
            var areasGroupPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            var areasGroupPublicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            var areas = areasGroupPrivateState.ServerLandClaimsAreas;
            var isFounderDemoPlayer = areasGroupPublicState.IsFounderDemoPlayer;
            var decayDelayDuration = ServerGetDecayDelayDurationForLandClaimAreas(areas,
                isFounderDemoPlayer,
                out var normalDecayDelayDuration);

            return new LandClaimsGroupDecayInfo(decayDelayDuration,
                                                decayDuration: StructureConstants.DecayDurationSeconds,
                                                isFounderDemoPlayer,
                                                normalDecayDelayDuration);
        }

        [RemoteCallSettings(timeInterval: 2)]
        private string ServerRemote_SetAreaOwners(ILogicObject area, List<string> newOwners)
        {
            var owner = ServerRemoteContext.Character;
            if (!Server.World.IsInPrivateScope(area, owner))
            {
                throw new Exception("Cannot interact with the land claim object as the area is not in private scope: "
                                    + area);
            }

            if (LandClaimAreasGroup.GetPublicState(SharedGetLandClaimAreasGroup(area))
                                   .ServerFaction is not null)
            {
                throw new Exception("Cannot change owners of the faction land claim");
            }

            if (newOwners.Count > RateLandClaimOwnersMax.SharedValue)
            {
                return WorldObjectOwnersSystem.DialogCannotSetOwners_AccessListSizeLimitExceeded;
            }

            var privateState = LandClaimArea.GetPrivateState(area);
            var currentOwners = privateState.DirectLandOwners;

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
                if (playerToAdd is null)
                {
                    return CoreStrings.PlayerNotFound;
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
                if (removedPlayer is null)
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

            /// <summary>
            /// Is the base was founded by a demo player?
            /// In that case a shortened decay duration should apply.
            /// </summary>
            public readonly bool IsFounderDemoPlayer;

            /// <summary>
            /// In case of the base founded by a demo player,
            /// this number would contain a non-shortened decay duration.
            /// </summary>
            public readonly double NormalDecayDelayDuration;

            public LandClaimsGroupDecayInfo(
                double decayDelayDuration,
                double decayDuration,
                bool isFounderDemoPlayer,
                double normalDecayDelayDuration)
            {
                this.DecayDelayDuration = decayDelayDuration;
                this.DecayDuration = decayDuration;
                this.IsFounderDemoPlayer = isFounderDemoPlayer;
                this.NormalDecayDelayDuration = normalDecayDelayDuration;
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
                    sharedLandClaimAreasCache = null;
                    ClientLandClaimAreaManager.AddArea(logicObject);
                }
            }

            private static void ClientObjectLeftScopeHandler(IGameObjectWithProto obj)
            {
                if (obj is ILogicObject logicObject
                    && obj.ProtoGameObject is LandClaimArea)
                {
                    sharedLandClaimAreas.Remove(logicObject);
                    sharedLandClaimAreasCache = null;
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
                foreach (var area in Api.Client.World.GetGameObjectsOfProto<ILogicObject, LandClaimArea>())
                {
                    sharedLandClaimAreas.Add(area);
                    ClientLandClaimAreaManager.AddArea(area);
                }

                sharedLandClaimAreasCache = null;
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

                sharedLandClaimAreasCache = null;
            }
        }
    }
}