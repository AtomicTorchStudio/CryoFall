namespace AtomicTorch.CBND.CoreMod.Systems.LandClaimShield
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.RaidingProtection;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;
    using static LandClaimShieldProtectionConstants;

    [PrepareOrder(afterType: typeof(ServerTimersSystem))]
    public class LandClaimShieldProtectionSystem : ProtoSystem<LandClaimShieldProtectionSystem>
    {
        [NotLocalizable]
        public override string Name => "Land claim shield protection system";

        public static void ClientActivateShield(ILogicObject areasGroup)
        {
            var status = SharedGetShieldPublicStatus(areasGroup);
            if (status != ShieldProtectionStatus.Inactive)
            {
                Logger.Warning("The shield is already active or activating");
                return;
            }

            if (LandClaimSystem.SharedIsAreasGroupUnderRaid(areasGroup))
            {
                NotificationSystem.ClientShowNotification(CoreStrings.ShieldProtection_CannotActivateDuringRaidBlock,
                                                          color: NotificationColor.Bad);
                return;
            }

            SharedGetShieldProtectionMaxStatsForBase(
                areasGroup,
                out var maxShieldDuration,
                out var electricityCapacity);

            var privateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            var electricityAmount = privateState.ShieldProtectionCurrentChargeElectricity;
            var durationEstimation = maxShieldDuration * electricityAmount / electricityCapacity;

            var message = string.Format(
                CoreStrings.ShieldProtection_ActivationConfirmation_ProtectionDuration_Format,
                ClientTimeFormatHelper.FormatTimeDuration(
                    durationEstimation,
                    appendSeconds: false));

            var cooldownRemains = SharedCalculateCooldownRemains(areasGroup);

            message += "[br][br]";
            if (cooldownRemains <= 0)
            {
                message += string.Format(CoreStrings.ShieldProtection_ActivationConfirmation_DelayDuration_Format,
                                         ClientTimeFormatHelper.FormatTimeDuration(
                                             SharedActivationDuration,
                                             appendSeconds: true));
            }
            else
            {
                message += string.Format(
                    CoreStrings.ShieldProtection_ActivationConfirmation_DelayDurationWithCooldown_Format,
                    ClientTimeFormatHelper.FormatTimeDuration(
                        SharedActivationDuration + cooldownRemains,
                        appendSeconds: true));
            }

            message += "[br][br]";
            message += string.Format(CoreStrings.ShieldProtection_DeactivationNotes_Format,
                                     ClientTimeFormatHelper.FormatTimeDuration(
                                         SharedCooldownDuration,
                                         appendSeconds: false));

            message += "[br][br]";
            message += CoreStrings.ShieldProtection_Description_9;

            DialogWindow.ShowDialog(
                CoreStrings.ShieldProtection_Dialog_ConfirmActivation,
                message,
                okText: CoreStrings.ShieldProtection_Button_ActivateShield,
                okAction: () => Instance.CallServer(_ => _.ServerRemote_ActivateShield(areasGroup)),
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        public static void ClientDeactivateShield(ILogicObject areasGroup)
        {
            var status = SharedGetShieldPublicStatus(areasGroup);
            if (status == ShieldProtectionStatus.Inactive)
            {
                Logger.Important("The shield is already inactive");
                return;
            }

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(
                DialogWindow.CreateTextElement(
                    string.Format(CoreStrings.ShieldProtection_DeactivationNotes_Format,
                                  ClientTimeFormatHelper.FormatTimeDuration(
                                      SharedCooldownDuration,
                                      appendSeconds: false)),
                    TextAlignment.Left));

            var accessRight = FactionMemberAccessRights.BaseShieldManagement;
            var areasGroupPublicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            var hasNoFactionPermission = !string.IsNullOrEmpty(areasGroupPublicState.FactionClanTag)
                                         && !FactionSystem.ClientHasAccessRight(accessRight);

            if (hasNoFactionPermission)
            {
                var message = FactionSystem.ClientHasFaction
                                  ? string.Format(
                                      CoreStrings.Faction_Permission_Required_Format,
                                      accessRight.GetDescription())
                                  : CoreStrings.Faction_ErrorDontHaveFaction;

                var textElement = DialogWindow.CreateTextElement("[br]" + message,
                                                                 TextAlignment.Left);

                textElement.Foreground = Client.UI.GetApplicationResource<Brush>("BrushColorRed6");
                textElement.FontWeight = FontWeights.Bold;
                stackPanel.Children.Add(textElement);
            }

            var dialog = DialogWindow.ShowDialog(
                CoreStrings.ShieldProtection_Dialog_ConfirmDeactivation,
                stackPanel,
                okText: CoreStrings.ShieldProtection_Button_DeactivateShield,
                okAction: () => Instance.CallServer(_ => _.ServerRemote_DeactivateShield(areasGroup)),
                cancelAction: () => { },
                focusOnCancelButton: true);

            if (hasNoFactionPermission)
            {
                dialog.ButtonOk.IsEnabled = false;
            }
        }

        public static void ClientRechargeShield(ILogicObject areasGroup, double targetChargeElectricity)
        {
            Api.Assert(targetChargeElectricity > 0, "Incorrect charge amount");
            var electricityCost = CalculateRequiredElectricityCost(areasGroup,
                                                                   targetChargeElectricity);
            if (electricityCost <= 0)
            {
                return;
            }

            var state = SharedGetShieldPublicStatus(areasGroup);
            if (state == ShieldProtectionStatus.Active)
            {
                throw new Exception("Cannot recharge active shield. It should be disabled.");
            }

            if (!PowerGridSystem.ServerBaseHasCharge(areasGroup, electricityCost))
            {
                var message = PowerGridSystem.SetPowerModeResult.NotEnoughPower.GetDescription();
                NotificationSystem.ClientShowNotification(
                    message,
                    null,
                    color: NotificationColor.Bad);
                return;
            }

            Instance.CallServer(_ => _.ServerRemote_RechargeShield(areasGroup, targetChargeElectricity));
            Logger.Important(
                $"Sent request to charge shield to {targetChargeElectricity:F2} (+{electricityCost:F2} EU)");
        }

        public static double SharedCalculateCooldownRemains(ILogicObject areasGroup)
        {
            var privateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            var time = IsServer
                           ? Server.Game.FrameTime
                           : Client.CurrentGame.ServerFrameTimeApproximated;
            return Math.Max(0, privateState.ShieldProtectionCooldownExpirationTime - time);
        }

        public static double SharedCalculateShieldEstimatedDuration(
            double charge,
            double durationMax,
            double electricityCapacity)
        {
            return durationMax * charge / electricityCapacity;
        }

        public static bool SharedCanActivateRaidblock(in RectangleInt targetObjectBounds, bool showClientNotification)
        {
            if (!SharedIsEnabled)
            {
                return true;
            }

            SharedFindLandClaimArea(targetObjectBounds,
                                    out _,
                                    out var isThereAnyLandClaimAreaWithShield);

            if (!isThereAnyLandClaimAreaWithShield)
            {
                // allow damage to structures in land claim areas without shield
                return true;
            }

            // don't allow damage as the structure is inside the shield-protected land claim area
            if (IsClient && showClientNotification)
            {
                ClientShowNotificationProtectedWithShield(isDamageCheck: false);
            }

            return false;
        }

        public static bool SharedCanRaid(IStaticWorldObject targetObject, bool showClientNotification)
        {
            if (!SharedIsEnabled)
            {
                return true;
            }

            switch (targetObject.ProtoGameObject)
            {
                case IProtoObjectWall:
                case IProtoObjectDoor:
                case IProtoObjectTurret:
                case ObjectPsionicFieldGenerator:
                    // only doors, walls, and defense structures are protected
                    break;

                default:
                    // other objects could be damaged always
                    return true;
            }

            // the raiding window is not now so we need to perform some checks to ensure the damage is allowed
            var targetObjectBounds = targetObject.Bounds;
            SharedFindLandClaimArea(targetObjectBounds,
                                    out var isThereAnyLandClaimArea,
                                    out var isThereAnyLandClaimAreaWithShield);

            if (!isThereAnyLandClaimArea)
            {
                // always allow damage to structures outside of land claim areas
                return true;
            }

            if (!isThereAnyLandClaimAreaWithShield)
            {
                // allow damage to structures in land claim areas without shield
                return true;
            }

            // don't allow damage as the structure is inside the shield-protected land claim area
            if (IsClient && showClientNotification)
            {
                ClientShowNotificationProtectedWithShield(isDamageCheck: true);
            }

            return false;
        }

        public static void SharedGetShieldProtectionMaxStatsForBase(
            ILogicObject areasGroup,
            out double maxDuration,
            out double electricityCapacity)
        {
            var areas = IsServer
                            ? LandClaimAreasGroup.GetPrivateState(areasGroup).ServerLandClaimsAreas
                            : LandClaimSystem.ClientGetKnownAreasForGroup(areasGroup);

            maxDuration = 0;
            electricityCapacity = 0;

            foreach (var area in areas)
            {
                IStaticWorldObject worldObject;

                if (IsServer)
                {
                    worldObject = LandClaimArea.GetPrivateState(area).ServerLandClaimWorldObject;
                }
                else
                {
                    worldObject = ClientFindLandClaimForArea(area);
                    if (worldObject is null)
                    {
                        continue;
                    }
                }

                var protoObjectLandClaim = (IProtoObjectLandClaim)worldObject.ProtoStaticWorldObject;
                maxDuration = Math.Max(protoObjectLandClaim.ShieldProtectionDuration,
                                       maxDuration);
                electricityCapacity = Math.Max(protoObjectLandClaim.ShieldProtectionTotalElectricityCost,
                                               electricityCapacity);
            }

            static IStaticWorldObject ClientFindLandClaimForArea(ILogicObject area)
            {
                foreach (var worldObject
                    in Client.World.GetGameObjectsOfProto<IStaticWorldObject, IProtoObjectLandClaim>())
                {
                    if (ReferenceEquals(area,
                                        worldObject.GetPublicState<ObjectLandClaimPublicState>()
                                                   .LandClaimAreaObject))
                    {
                        // found it
                        return worldObject;
                    }
                }

                return null;
            }
        }

        public static ShieldProtectionStatus SharedGetShieldPublicStatus(ILogicObject areasGroup)
        {
            return LandClaimAreasGroup.GetPublicState(areasGroup).Status;
        }

        public static bool SharedIsAreaUnderShieldProtection(ILogicObject area)
        {
            if (!SharedIsEnabled)
            {
                return false;
            }

            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(area);
            return SharedGetShieldPublicStatus(areasGroup) == ShieldProtectionStatus.Active;
        }

        public static bool SharedIsUnderShieldProtection(IStaticWorldObject worldObject)
        {
            if (!SharedIsEnabled)
            {
                return false;
            }

            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(worldObject);
            if (areasGroup is null)
            {
                return false;
            }

            return SharedGetShieldPublicStatus(areasGroup) == ShieldProtectionStatus.Active;
        }

        public static void SharedSendNotificationActionForbiddenUnderShieldProtection(ICharacter character)
        {
            if (IsClient)
            {
                Instance.ClientRemote_ShowNotificationActionForbiddenUnderShieldProtection();
            }
            else
            {
                Instance.CallClient(character,
                                    _ => _.ClientRemote_ShowNotificationActionForbiddenUnderShieldProtection());
            }
        }

        [RemoteCallSettings(timeInterval: RemoteCallSettingsAttribute.MaxTimeInterval)]
        public (bool SharedIsEnabled, double SharedActivationDuration, double SharedCooldownDuration)
            ServerRemote_GetSettings()
        {
            return (SharedIsEnabled, SharedActivationDuration, SharedCooldownDuration);
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            // cannot immediately process as the world objects currently are not loaded
            // process after delay
            ServerTimersSystem.AddAction(0, ProcessAllBases);

            if (!SharedIsEnabled)
            {
                return;
            }

            LandClaimSystem.ServerAreasGroupCreated += ServerAreasGroupCreatedHandler;
            LandClaimSystem.ServerBaseMerge += ServerBaseMergeHandler;
            LandClaimSystem.ServerRaidBlockStartedOrExtended += ServerRaidBlockStartedOrExtendedHandler;
            TriggerEveryFrame.ServerRegister(ServerUpdate, this.ShortId);

            static void ProcessAllBases()
            {
                var isSystemEnabled = SharedIsEnabled;

                // Apply lockdown to all doors inside the shield-protected bases
                // as doors don't store the blocked-by-shield status.
                var allGroups = Server.World.GetGameObjectsOfProto<ILogicObject, LandClaimAreasGroup>();
                foreach (var areasGroup in allGroups)
                {
                    if (isSystemEnabled)
                    {
                        if (SharedGetShieldPublicStatus(areasGroup) == ShieldProtectionStatus.Active)
                        {
                            ServerSetDoorsLockdownStatus(areasGroup, areDoorsBlocked: true);
                        }
                    }
                    else // when system inactive all shields should be disabled
                    if (SharedGetShieldPublicStatus(areasGroup) != ShieldProtectionStatus.Inactive)
                    {
                        ServerDeactivateShield(areasGroup);
                    }
                }
            }
        }

        private static double CalculateRequiredElectricityCost(
            ILogicObject areasGroup,
            double targetElectricityAmount)
        {
            if (targetElectricityAmount <= 0)
            {
                return 0;
            }

            SharedGetShieldProtectionMaxStatsForBase(areasGroup,
                                                     out _,
                                                     out var electricityCapacity);

            targetElectricityAmount = Math.Min(electricityCapacity, targetElectricityAmount);

            var privateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            var electricityCost = targetElectricityAmount - privateState.ShieldProtectionCurrentChargeElectricity;
            return Math.Max(0, electricityCost);
        }

        private static void ClientShowNotificationProtectedWithShield(bool isDamageCheck)
        {
            if (isDamageCheck)
            {
                NotificationSystem.ClientShowNotification(
                    RaidingProtectionSystem.Notification_CannotDamageUnderRaidingProtection_Title,
                    CoreStrings.ShieldProtection_NotificationProtected_Message);
            }
            else
            {
                NotificationSystem.ClientShowNotification(
                    CoreStrings.ShieldProtection_NotificationProtected_Message);
            }
        }

        private static void ServerAreasGroupCreatedHandler(ILogicObject areasGroup)
        {
            var privateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            privateState.ShieldProtectionCooldownExpirationTime = Server.Game.FrameTime + SharedCooldownDuration;
        }

        private static void ServerBaseMergeHandler(ILogicObject areasGroupFrom, ILogicObject areasGroupTo)
        {
            var fromPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroupFrom);
            var toPrivateState = LandClaimAreasGroup.GetPrivateState(areasGroupTo);

            toPrivateState.ShieldProtectionCurrentChargeElectricity = Math.Max(
                toPrivateState.ShieldProtectionCurrentChargeElectricity,
                fromPrivateState.ShieldProtectionCurrentChargeElectricity);

            toPrivateState.ShieldProtectionCooldownExpirationTime = Math.Max(
                toPrivateState.ShieldProtectionCooldownExpirationTime,
                fromPrivateState.ShieldProtectionCooldownExpirationTime);

            ServerDeactivateShield(areasGroupFrom);
            ServerDeactivateShield(areasGroupTo);
        }

        private static void ServerDeactivateShield(ILogicObject areasGroup, ICharacter logByPlayer = null)
        {
            var privateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            var publicState = LandClaimAreasGroup.GetPublicState(areasGroup);

            if (publicState.Status == ShieldProtectionStatus.Inactive)
            {
                return;
            }

            Logger.Important("Shield deactivated: " + areasGroup, logByPlayer);
            publicState.Status = ShieldProtectionStatus.Inactive;
            publicState.ShieldActivationTime = 0;
            publicState.ShieldEstimatedExpirationTime = 0;
            privateState.ShieldProtectionCooldownExpirationTime = Server.Game.FrameTime + SharedCooldownDuration;
            ServerSetDoorsLockdownStatus(areasGroup, areDoorsBlocked: false);
        }

        private static void ServerRaidBlockStartedOrExtendedHandler(
            ILogicObject area,
            ICharacter raiderCharacter,
            bool isNewRaidBlock,
            bool isStructureDestroyed)
        {
            if (!isStructureDestroyed)
            {
                return;
            }

            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(area);
            var publicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            if (publicState.Status != ShieldProtectionStatus.Activating)
            {
                return;
            }

            Logger.Important($"Shield activation cancelled due to the raidblock: {areasGroup} by {raiderCharacter}");
            ServerDeactivateShield(areasGroup);

            var allOwners = LandClaimAreasGroup.GetPrivateState(areasGroup)
                                               .ServerLandClaimsAreas
                                               .SelectMany(a => LandClaimArea.GetPrivateState(a).ServerGetLandOwners())
                                               .Distinct(StringComparer.Ordinal)
                                               .Select(a => Server.Characters.GetPlayerCharacter(a))
                                               .ToList();
            Instance.CallClient(allOwners, _ => _.ClientRemote_ShieldActivationCancelledDueToRaidblock());
        }

        private static void ServerSetDoorsLockdownStatus(ILogicObject areasGroup, bool areDoorsBlocked)
        {
            var boundingBox = LandClaimSystem.SharedGetLandClaimGroupBoundingArea(areasGroup);
            if (boundingBox == default)
            {
                return;
            }

            using var areasBounds = Api.Shared.GetTempList<RectangleInt>();
            foreach (var area in LandClaimAreasGroup.GetPrivateState(areasGroup)
                                                    .ServerLandClaimsAreas)
            {
                var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area);
                areasBounds.Add(bounds);
            }

            var selectedDoors = new HashSet<IStaticWorldObject>();
            SelectDoorsInsideBase(selectedDoors);

            foreach (var worldObjectDoor in selectedDoors)
            {
                var doorPrivateState = worldObjectDoor.GetPrivateState<ObjectDoorPrivateState>();
                doorPrivateState.IsBlockedByShield = areDoorsBlocked;
            }

            Logger.Important(
                string.Format("Base lockdown - {0} doors were {1} in {2}",
                              selectedDoors.Count,
                              areDoorsBlocked ? "blocked" : "unblocked",
                              areasGroup));

            void SelectDoorsInsideBase(HashSet<IStaticWorldObject> result)
            {
                var allDoors = Server.World.GetStaticWorldObjectsOfProtoInBounds<IProtoObjectDoor>(boundingBox);
                foreach (var worldObjectDoor in allDoors)
                {
                    var position = worldObjectDoor.TilePosition;
                    foreach (var areaBounds in areasBounds.AsList())
                    {
                        if (!areaBounds.Contains(position))
                        {
                            continue;
                        }

                        // it's important to use "if not contains" check here
                        // as the enumeration could provide the same object several times
                        result.AddIfNotContains(worldObjectDoor);
                        break;
                    }
                }
            }
        }

        private static void ServerUpdate()
        {
            // perform update once per 1 second per base
            const double spreadDeltaTime = 1;
            var serverTime = Server.Game.FrameTime;

            using var tempListAreasGroups = Api.Shared.GetTempList<ILogicObject>();
            Api.GetProtoEntity<LandClaimAreasGroup>()
               .EnumerateGameObjectsWithSpread(tempListAreasGroups.AsList(),
                                               spreadDeltaTime: spreadDeltaTime,
                                               Server.Game.FrameNumber,
                                               Server.Game.FrameRate);

            foreach (var areasGroup in tempListAreasGroups.AsList())
            {
                UpdateGroup(areasGroup);
            }

            void UpdateGroup(ILogicObject areasGroup)
            {
                var privateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
                var publicState = LandClaimAreasGroup.GetPublicState(areasGroup);

                // Commented-out - now the shield activation is cancelled only if there is a structure is destroyed
                // inside the land claim area (no matter the raid block).
                // See method ServerRaidBlockStartedOrExtendedHandler.
                /*if (LandClaimSystem.SharedIsAreasGroupUnderRaid(areasGroup))
                {
                    if (publicState.Status == ShieldProtectionStatus.Inactive)
                    {
                        return;
                    }

                    //if (publicState.Status == ShieldProtectionStatus.Active)
                    //{
                    //    // should be impossible - the raid block cannot activate on the base with active shield
                    //    continue;
                    //}

                    Logger.Important("Shield activation cancelled due to the raidblock: " + areasGroup);
                    ServerDeactivateShield(areasGroup);

                    var allOwners = LandClaimAreasGroup.GetPrivateState(areasGroup)
                                                       .ServerLandClaimsAreas
                                                       .SelectMany(
                                                           a => LandClaimArea.GetPrivateState(a).ServerGetLandOwners())
                                                       .Distinct(StringComparer.Ordinal)
                                                       .Select(a => Server.Characters.GetPlayerCharacter(a))
                                                       .ToList();
                    Instance.CallClient(allOwners, _ => _.ClientRemote_ShieldActivationCancelledDueToRaidblock());
                    return;
                }*/

                double maxDuration, electricityCapacity;
                var status = SharedGetShieldPublicStatus(areasGroup);
                if (status == ShieldProtectionStatus.Activating
                    && serverTime >= publicState.ShieldActivationTime)
                {
                    // activation time reached!
                    Logger.Important("Shield activated: " + areasGroup);
                    publicState.Status = status = ShieldProtectionStatus.Active;

                    ServerSetDoorsLockdownStatus(areasGroup, areDoorsBlocked: true);

                    SharedGetShieldProtectionMaxStatsForBase(areasGroup,
                                                             out maxDuration,
                                                             out electricityCapacity);

                    publicState.ShieldEstimatedExpirationTime
                        = serverTime
                          + SharedCalculateShieldEstimatedDuration(
                              privateState.ShieldProtectionCurrentChargeElectricity,
                              maxDuration,
                              electricityCapacity);
                }

                if (status != ShieldProtectionStatus.Active)
                {
                    return;
                }

                // consume energy
                SharedGetShieldProtectionMaxStatsForBase(areasGroup,
                                                         out maxDuration,
                                                         out electricityCapacity);

                var charge = privateState.ShieldProtectionCurrentChargeElectricity;
                charge -= spreadDeltaTime * (electricityCapacity / maxDuration);
                if (charge > 0)
                {
                    privateState.ShieldProtectionCurrentChargeElectricity = charge;
                    return;
                }

                Logger.Important("Shield depleted: " + areasGroup);
                privateState.ShieldProtectionCurrentChargeElectricity = 0;
                ServerDeactivateShield(areasGroup);
            }
        }

        private static bool ServerValidateCharacterAccessToAreasGroup(
            ILogicObject areasGroup,
            ICharacter character,
            bool requireFactionPermission,
            bool writeToLog = true)
        {
            var areas = LandClaimAreasGroup.GetPrivateState(areasGroup).ServerLandClaimsAreas;
            foreach (var area in areas)
            {
                if (LandClaimSystem.ServerIsOwnedArea(area, character, requireFactionPermission))
                {
                    return true;
                }
            }

            if (writeToLog)
            {
                Logger.Warning("Player has no access to the land claim: " + areasGroup,
                               character);
            }

            return false;
        }

        private static void SharedFindLandClaimArea(
            RectangleInt targetObjectBounds,
            out bool isThereAnyLandClaimArea,
            out bool isThereAnyLandClaimAreaWithShield)
        {
            isThereAnyLandClaimArea = false;
            isThereAnyLandClaimAreaWithShield = false;
            using var landClaimAreas = Api.Shared.GetTempList<ILogicObject>();
            LandClaimSystem.SharedGetAreasInBounds(targetObjectBounds,
                                                   landClaimAreas,
                                                   addGracePadding: false);
            foreach (var area in landClaimAreas.AsList())
            {
                isThereAnyLandClaimArea = true;
                var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(area);
                var status = SharedGetShieldPublicStatus(areasGroup);

                isThereAnyLandClaimAreaWithShield = status == ShieldProtectionStatus.Active;
                if (isThereAnyLandClaimAreaWithShield)
                {
                    return;
                }
            }
        }

        private void ClientRemote_ShieldActivationCancelledDueToRaidblock()
        {
            NotificationSystem.ClientShowNotification(CoreStrings.ShieldProtection_CancelledActivationDueToRaidBlock,
                                                      color: NotificationColor.Bad);
        }

        private void ClientRemote_ShowNotificationActionForbiddenUnderShieldProtection()
        {
            NotificationSystem.ClientShowNotification(
                CoreStrings.Notification_ActionForbidden,
                CoreStrings.ShieldProtection_ActionRestrictedBaseUnderShieldProtection,
                color: NotificationColor.Bad);
        }

        [RemoteCallSettings(timeInterval: 2)]
        private void ServerRemote_ActivateShield(ILogicObject areasGroup)
        {
            if (!SharedIsEnabled)
            {
                return;
            }

            var character = ServerRemoteContext.Character;
            if (!ServerValidateCharacterAccessToAreasGroup(areasGroup,
                                                           character,
                                                           requireFactionPermission: false))
            {
                return;
            }

            var currentInteractionObject = InteractionCheckerSystem.SharedGetCurrentInteraction(character);
            if (currentInteractionObject is null
                || (areasGroup
                    != LandClaimSystem.SharedGetLandClaimAreasGroup(
                        ProtoObjectLandClaim.GetPublicState((IStaticWorldObject)currentInteractionObject)
                                            .LandClaimAreaObject)))
            {
                Logger.Warning("Player not interacting with the land claim to activate the shield: " + areasGroup,
                               character);
                return;
            }

            var status = SharedGetShieldPublicStatus(areasGroup);
            if (status != ShieldProtectionStatus.Inactive)
            {
                Logger.Info("The shield is already active or activating: " + areasGroup);
                return;
            }

            if (LandClaimShieldProtectionHelper.SharedIsLandClaimInsideAnotherBase(areasGroup))
            {
                Logger.Warning("The base is inside another base so it cannot use the shield: " + areasGroup);
                return;
            }

            SharedGetShieldProtectionMaxStatsForBase(areasGroup,
                                                     out _,
                                                     out var electricityCapacity);
            if (electricityCapacity <= 0)
            {
                Logger.Warning("The shield is not available for this base (too low tier): " + areasGroup);
                return;
            }

            if (LandClaimSystem.SharedIsAreasGroupUnderRaid(areasGroup))
            {
                Logger.Warning("Cannot active the shield due to the raid block: " + areasGroup);
                return;
            }

            var privateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            var publicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            if (privateState.ShieldProtectionCurrentChargeElectricity <= 0)
            {
                Logger.Warning("The shield doesn't have charge: " + areasGroup);
                return;
            }

            if (!string.IsNullOrEmpty(publicState.FactionClanTag)
                && !FactionSystem.ServerHasAccessRights(character,
                                                        FactionMemberAccessRights.BaseShieldManagement,
                                                        out _))
            {
                Logger.Warning("Has no faction permission to manage the shield");
                return;
            }

            var cooldownRemains = SharedCalculateCooldownRemains(areasGroup);
            publicState.ShieldActivationTime = Server.Game.FrameTime
                                               + SharedActivationDuration
                                               + cooldownRemains;

            publicState.Status = ShieldProtectionStatus.Activating;

            Logger.Important("Shield activation scheduled after "
                             + SharedActivationDuration.ToString("F2")
                             + " seconds for "
                             + areasGroup,
                             character);
        }

        [RemoteCallSettings(timeInterval: 2)]
        private void ServerRemote_DeactivateShield(ILogicObject areasGroup)
        {
            var character = ServerRemoteContext.Character;
            if (!ServerValidateCharacterAccessToAreasGroup(areasGroup,
                                                           character,
                                                           requireFactionPermission: false))
            {
                return;
            }

            var status = SharedGetShieldPublicStatus(areasGroup);
            if (status == ShieldProtectionStatus.Inactive)
            {
                Logger.Info("The shield is already inactive: " + areasGroup);
                return;
            }

            var publicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            if (!string.IsNullOrEmpty(publicState.FactionClanTag)
                && !FactionSystem.ServerHasAccessRights(character,
                                                        FactionMemberAccessRights.BaseShieldManagement,
                                                        out _))
            {
                Logger.Warning("Has no faction permission to manage the shield");
                return;
            }

            ServerDeactivateShield(areasGroup, character);
        }

        [RemoteCallSettings(timeInterval: 1)]
        private void ServerRemote_RechargeShield(
            ILogicObject areasGroup,
            double totalChargeElectricity)
        {
            if (!SharedIsEnabled)
            {
                return;
            }

            var character = ServerRemoteContext.Character;
            var currentInteractionObject = InteractionCheckerSystem.SharedGetCurrentInteraction(character);
            if (currentInteractionObject is null
                || (areasGroup
                    != LandClaimSystem.SharedGetLandClaimAreasGroup(
                        ProtoObjectLandClaim.GetPublicState((IStaticWorldObject)currentInteractionObject)
                                            .LandClaimAreaObject)))
            {
                Logger.Warning("Player not interacting with the land claim to recharge the shield: " + areasGroup,
                               character);
                return;
            }

            if (!ServerValidateCharacterAccessToAreasGroup(areasGroup,
                                                           character,
                                                           requireFactionPermission: false))
            {
                return;
            }

            SharedGetShieldProtectionMaxStatsForBase(areasGroup,
                                                     out _,
                                                     out var electricityCapacity);
            if (electricityCapacity <= 0)
            {
                Logger.Warning("The shield is not available for this base (too low tier): " + areasGroup);
                return;
            }

            var state = SharedGetShieldPublicStatus(areasGroup);
            if (state == ShieldProtectionStatus.Active)
            {
                throw new Exception("Cannot recharge active shield. It should be disabled.");
            }

            var electricityCost = CalculateRequiredElectricityCost(areasGroup,
                                                                   totalChargeElectricity);
            if (electricityCost <= 0)
            {
                // recharging is not required
                Logger.Important($"Recharging is not required: {totalChargeElectricity:F2} (+{electricityCost:F2} EU)");
                return;
            }

            if (!PowerGridSystem.ServerBaseHasCharge(areasGroup, electricityCost))
            {
                throw new Exception("Not enough power in the power grid to charge the shield");
            }

            PowerGridSystem.ServerDeductBaseCharge(areasGroup, electricityCost);
            var privateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            privateState.ShieldProtectionCurrentChargeElectricity = totalChargeElectricity;

            Logger.Important($"Shield recharged to: {totalChargeElectricity:F2} (+{electricityCost:F2} EU)");
        }
    }
}