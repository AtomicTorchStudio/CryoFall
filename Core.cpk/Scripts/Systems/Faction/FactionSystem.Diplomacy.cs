namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public partial class FactionSystem
    {
        private static NetworkSyncDictionary<string, FactionDiplomacyStatus> clientCurrentFactionDiplomacyStatuses;

        public static event Action<(string clanTag, FactionDiplomacyStatus status)> ClientFactionDiplomacyStatusChanged;

        [Serializable]
        [RemoteEnum]
        public enum FactionDiplomacyStatusChangeRequest : byte
        {
            Neutral = 0,

            Enemy = 1,

            Ally = 2
        }

        [RemoteEnum]
        private enum FactionDiplomacyRequestResult : byte
        {
            Success = 0,

            ErrorUnknown = 1,

            ErrorUnknownClanTag = 2
        }

        /// <summary>
        /// Diplomacy (proposing alliances/declaring wars) is available only on PvP servers.
        /// </summary>
        public static bool SharedIsDiplomacyFeatureAvailable
            => !PveSystem.SharedIsPve(true);

        public static Task<IReadOnlyList<FactionDiplomacyPublicStatusEntry>> ClientFactionDiplomacyPublicInfo(
            string clanTag)
        {
            return Instance.CallServer(_ => _.ServerRemote_GetFactionDiplomacyPublicInfo(clanTag));
        }

        public static FactionDiplomacyStatus ClientGetCurrentFactionDiplomacyStatus(string otherFactionClanTag)
        {
            if (ClientCurrentFaction is null
                || string.IsNullOrEmpty(otherFactionClanTag))
            {
                return FactionDiplomacyStatus.Neutral;
            }

            if (ClientCurrentFactionClanTag == otherFactionClanTag)
            {
                Logger.Error(
                    $"Same faction provided in {nameof(FactionSystem)}.{nameof(ClientGetCurrentFactionDiplomacyStatus)}");
                return FactionDiplomacyStatus.Neutral;
            }

            return SharedGetFactionDiplomacyStatus(ClientCurrentFaction, otherFactionClanTag);
        }

        public static bool ClientIsValidClanTagForRequest(
            string clanTag,
            bool showErrorNotification = true)
        {
            if (!string.IsNullOrEmpty(clanTag)
                && clanTag.Length <= 4)
            {
                return true;
            }

            if (showErrorNotification)
            {
                NotificationSystem.ClientShowNotification(CoreStrings.ClanTag_Invalid,
                                                          color: NotificationColor.Bad);
            }

            return false;
        }

        public static void ClientOfficerAcceptAlliance(string otherFactionClanTag)
        {
            ClientOfficerSetDiplomacyStatus(otherFactionClanTag, FactionDiplomacyStatusChangeRequest.Ally);
        }

        public static void ClientOfficerCancelOutgoingAllianceRequest(string otherFactionClanTag)
        {
            otherFactionClanTag = ClientSanitizeClanTag(otherFactionClanTag);
            ClientValidateHasAccessRights(FactionMemberAccessRights.DiplomacyManagement);
            Instance.CallServer(
                _ => _.ServerRemote_OfficerCancelOutgoingAllianceRequest(otherFactionClanTag));
        }

        public static async void ClientOfficerProposeAlliance(string otherFactionClanTag)
        {
            SharedValidateAlliancesEnabled();

            otherFactionClanTag = ClientSanitizeClanTag(otherFactionClanTag);
            if (!ClientIsValidClanTagForRequest(otherFactionClanTag))
            {
                return;
            }

            if (Faction.GetPrivateState(ClientCurrentFaction)
                       .OutgoingFactionAllianceRequests
                       .TryGetValue(otherFactionClanTag, out var existingRequest)
                && existingRequest.NewRequestCooldownDate > Api.Client.CurrentGame.ServerFrameTimeApproximated)
            {
                NotificationSystem.ClientShowNotification(
                    string.Format(CoreStrings.Faction_Diplomacy_ProposeAlliance_ErrorCooldown_Format,
                                  ClientTimeFormatHelper.FormatTimeDuration(
                                      existingRequest.NewRequestCooldownDate
                                      - Api.Client.CurrentGame.ServerFrameTimeApproximated)),
                    color: NotificationColor.Bad);
                return;
            }

            ClientValidateHasAccessRights(FactionMemberAccessRights.DiplomacyManagement);
            var result = await Instance.CallServer(
                             _ => _.ServerRemote_OfficerProposeAlliance(otherFactionClanTag));

            ClientProcessFactionRequestResult(result);
        }

        public static void ClientOfficerRejectAlliance(string otherFactionClanTag)
        {
            otherFactionClanTag = ClientSanitizeClanTag(otherFactionClanTag);
            ClientValidateHasAccessRights(FactionMemberAccessRights.DiplomacyManagement);
            Instance.CallServer(
                _ => _.ServerRemote_OfficerRejectAlliance(otherFactionClanTag));
        }

        public static async void ClientOfficerSetDiplomacyStatus(
            string otherFactionClanTag,
            FactionDiplomacyStatusChangeRequest statusChangeRequest)
        {
            otherFactionClanTag = ClientSanitizeClanTag(otherFactionClanTag);
            if (!ClientIsValidClanTagForRequest(otherFactionClanTag))
            {
                return;
            }

            ClientValidateHasAccessRights(FactionMemberAccessRights.DiplomacyManagement);
            var result = await Instance.CallServer(
                             _ => _.ServerRemote_OfficerSetDiplomacyStatus(otherFactionClanTag,
                                                                           statusChangeRequest));

            ClientProcessFactionRequestResult(result);
        }

        public static string ClientSanitizeClanTag(string otherFactionClanTag)
        {
            otherFactionClanTag = otherFactionClanTag?.Trim()
                                                     .ToUpperInvariant()
                                  ?? string.Empty;
            return otherFactionClanTag;
        }

        public static FactionDiplomacyStatus ServerGetFactionDiplomacyStatus(
            ILogicObject faction,
            ILogicObject otherFaction)
        {
            Api.Assert(faction != otherFaction, "The factions should be different");
            var otherFactionClanTag = SharedGetClanTag(otherFaction);
            return SharedGetFactionDiplomacyStatus(faction, otherFactionClanTag);
        }

        /// <summary>
        /// Useful when need to break an alliance or end the war.
        /// </summary>
        public static void ServerSetFactionDiplomacyStatusNeutral(ILogicObject faction, ILogicObject otherFaction)
        {
            ServerSetFactionDiplomacyStatusInternal(faction,      otherFaction, FactionDiplomacyStatus.Neutral);
            ServerSetFactionDiplomacyStatusInternal(otherFaction, faction,      FactionDiplomacyStatus.Neutral);
        }

        public static FactionDiplomacyStatus SharedGetFactionDiplomacyStatus(
            ILogicObject faction,
            string otherFactionClanTag)
        {
            Api.Assert(faction is not null, "Faction should not be null");
            if (otherFactionClanTag is null)
            {
                return FactionDiplomacyStatus.Neutral;
            }

            Api.Assert(Faction.GetPublicState(faction).ClanTag != otherFactionClanTag,
                       "The factions should be different");

            return Faction.GetPrivateState(faction)
                          .FactionDiplomacyStatuses
                          .TryGetValue(otherFactionClanTag, out var status)
                       ? status
                       : FactionDiplomacyStatus.Neutral;
        }

        private static void ClientCurrentFactionDiplomacyStatusesPairRemovedHandler(
            NetworkSyncDictionary<string, FactionDiplomacyStatus> source,
            string clanTag)
        {
            ClientFactionDiplomacyStatusChanged?.Invoke((clanTag, FactionDiplomacyStatus.Neutral));
        }

        private static void ClientProcessFactionRequestResult(FactionDiplomacyRequestResult result)
        {
            if (result == FactionDiplomacyRequestResult.ErrorUnknownClanTag)
            {
                NotificationSystem.ClientShowNotification(CoreStrings.Faction_ErrorFactionNotFound,
                                                          color: NotificationColor.Bad);
            }
        }

        private static void CurrentFactionDiplomacyStatusesPairSetHandler(
            NetworkSyncDictionary<string, FactionDiplomacyStatus> source,
            string clanTag,
            FactionDiplomacyStatus value)
        {
            ClientFactionDiplomacyStatusChanged?.Invoke((clanTag, value));
        }

        private static void ServerResetDiplomacy(ILogicObject currentFaction)
        {
            var currentFactionClanTag = SharedGetClanTag(currentFaction);
            var currentFactionPrivateState = Faction.GetPrivateState(currentFaction);

            foreach (var otherFactionClanTag in currentFactionPrivateState.FactionDiplomacyStatuses.Keys)
            {
                try
                {
                    var otherFaction = ServerGetFactionByClanTag(otherFactionClanTag);
                    var otherFactionDiplomacyStatuses = Faction.GetPrivateState(otherFaction)
                                                               .FactionDiplomacyStatuses;
                    if (!otherFactionDiplomacyStatuses.TryGetValue(currentFactionClanTag,
                                                                   out var lastStatus))
                    {
                        continue;
                    }

                    otherFactionDiplomacyStatuses.Remove(currentFactionClanTag);

                    if (lastStatus == FactionDiplomacyStatus.Neutral)
                    {
                        continue;
                    }

                    ServerAddLogEntry(otherFaction,
                                      new FactionLogEntryDiplomacyStatusChanged(
                                          clanTag: currentFactionClanTag,
                                          byOfficer: null,
                                          fromStatus: lastStatus,
                                          toStatus: FactionDiplomacyStatus.Neutral));
                }
                catch (Exception exception)
                {
                    Logger.Exception(exception);
                }
            }

            currentFactionPrivateState.FactionDiplomacyStatuses.Clear();

            foreach (var otherFactionClanTag in currentFactionPrivateState.IncomingFactionAllianceRequests.Keys)
            {
                try
                {
                    var otherFaction = ServerGetFactionByClanTag(otherFactionClanTag);
                    Faction.GetPrivateState(otherFaction)
                           .OutgoingFactionAllianceRequests
                           .Remove(currentFactionClanTag);
                }
                catch (Exception exception)
                {
                    Logger.Exception(exception);
                }
            }

            currentFactionPrivateState.IncomingFactionAllianceRequests.Clear();

            foreach (var otherFactionClanTag in currentFactionPrivateState.OutgoingFactionAllianceRequests.Keys)
            {
                try
                {
                    var otherFaction = ServerGetFactionByClanTag(otherFactionClanTag);
                    Faction.GetPrivateState(otherFaction)
                           .IncomingFactionAllianceRequests
                           .Remove(currentFactionClanTag);
                }
                catch (Exception exception)
                {
                    Logger.Exception(exception);
                }
            }

            currentFactionPrivateState.OutgoingFactionAllianceRequests.Clear();

            Logger.Info("Reset faction diplomacy: " + currentFaction);
        }

        /// <summary>
        /// Please note - it's not enough to call this method only for one faction.
        /// It should be called for the other faction as well to change their diplomacy status to current faction.
        /// </summary>
        private static void ServerSetFactionDiplomacyStatusInternal(
            ILogicObject faction,
            ILogicObject otherFaction,
            FactionDiplomacyStatus status,
            ICharacter byOfficer = null)
        {
            var otherFactionClanTag = SharedGetClanTag(otherFaction);
            var factionDiplomacyStatuses = Faction.GetPrivateState(faction)
                                                  .FactionDiplomacyStatuses;

            if (factionDiplomacyStatuses.TryGetValue(otherFactionClanTag, out var fromStatus))
            {
                if (status == fromStatus)
                {
                    // no change
                    return;
                }
            }
            else
            {
                fromStatus = FactionDiplomacyStatus.Neutral;
            }

            if (status == FactionDiplomacyStatus.Neutral)
            {
                factionDiplomacyStatuses.Remove(otherFactionClanTag);
            }
            else
            {
                factionDiplomacyStatuses[otherFactionClanTag] = status;
            }

            ServerAddLogEntry(faction,
                              new FactionLogEntryDiplomacyStatusChanged(
                                  clanTag: otherFactionClanTag,
                                  byOfficer: byOfficer,
                                  fromStatus: fromStatus,
                                  toStatus: status));

            Logger.Important(
                string.Format("Faction diplomacy status changed: {0} with {1}: {2}",
                              SharedGetClanTag(faction),
                              otherFactionClanTag,
                              status));
        }

        private static void ServerUpdateAllianceRequestsExpiration()
        {
            // perform update once per 10 seconds per faction
            const double spreadDeltaTime = 10;
            var currentTime = Server.Game.FrameTime;

            using var tempListFactions = Api.Shared.GetTempList<ILogicObject>();

            ProtoFaction.EnumerateGameObjectsWithSpread(tempListFactions.AsList(),
                                                        spreadDeltaTime: spreadDeltaTime,
                                                        Server.Game.FrameNumber,
                                                        Server.Game.FrameRate);

            foreach (var faction in tempListFactions.AsList())
            {
                var outgoingRequests = Faction.GetPrivateState(faction).OutgoingFactionAllianceRequests;
                outgoingRequests.ProcessAndRemoveByPair(
                    removeCondition: pair =>
                                     {
                                         var request = pair.Value;
                                         return request.IsRejected && currentTime > request.NewRequestCooldownDate
                                                || currentTime > request.ExpirationDate;
                                     },
                    removeCallback: pair =>
                                    {
                                        // remove incoming alliance request in the other faction
                                        var otherFaction = ServerGetFactionByClanTag(pair.Key);
                                        Faction.GetPrivateState(otherFaction)
                                               .IncomingFactionAllianceRequests
                                               .Remove(SharedGetClanTag(faction));
                                    });
            }
        }

        private static bool SharedIsDiplomacyStatusChangeRequired(
            FactionDiplomacyStatus status,
            FactionDiplomacyStatusChangeRequest request)
        {
            return request switch
            {
                FactionDiplomacyStatusChangeRequest.Neutral
                    when status == FactionDiplomacyStatus.Neutral
                    => false, // already neutral

                FactionDiplomacyStatusChangeRequest.Ally
                    when status == FactionDiplomacyStatus.Ally
                    => false, // already an ally

                FactionDiplomacyStatusChangeRequest.Enemy
                    when status == FactionDiplomacyStatus.EnemyMutual
                         || status == FactionDiplomacyStatus.EnemyDeclaredByCurrentFaction
                    => false, // already a declared enemy

                _ => true
            };
        }

        private static void SharedValidateAlliancesEnabled()
        {
            if (!FactionConstants.SharedPvpAlliancesEnabled)
            {
                throw new Exception("Alliances are not available on this server");
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 2)]
        private IReadOnlyList<FactionDiplomacyPublicStatusEntry> ServerRemote_GetFactionDiplomacyPublicInfo(
            string clanTag)
        {
            var faction = ServerGetFactionByClanTag(clanTag);
            var statuses = Faction.GetPrivateState(faction).FactionDiplomacyStatuses;
            if (statuses.Count == 0)
            {
                return Array.Empty<FactionDiplomacyPublicStatusEntry>();
            }

            var list = new List<FactionDiplomacyPublicStatusEntry>(statuses.Count);
            foreach (var entry in statuses)
            {
                switch (entry.Value)
                {
                    case FactionDiplomacyStatus.EnemyMutual:
                    case FactionDiplomacyStatus.EnemyDeclaredByCurrentFaction:
                    case FactionDiplomacyStatus.EnemyDeclaredByOtherFaction:
                        list.Add(new FactionDiplomacyPublicStatusEntry(entry.Key, isAlly: false));
                        break;

                    case FactionDiplomacyStatus.Ally:
                        list.Add(new FactionDiplomacyPublicStatusEntry(entry.Key, isAlly: true));
                        break;

                    default:
                    case FactionDiplomacyStatus.Neutral:
                        continue;
                }
            }

            return list;
        }

        [RemoteCallSettings(timeInterval: 2)]
        private void ServerRemote_OfficerCancelOutgoingAllianceRequest(string otherFactionClanTag)
        {
            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.DiplomacyManagement,
                                          out var officerFaction);

            var officerFactionClanTag = SharedGetClanTag(officerFaction);
            var officerFactionOutgoingRequests =
                Faction.GetPrivateState(officerFaction).OutgoingFactionAllianceRequests;

            var otherFaction = ServerGetFactionByClanTag(otherFactionClanTag);
            if (otherFaction == officerFaction)
            {
                throw new InvalidOperationException();
            }

            if (!officerFactionOutgoingRequests.TryGetValue(otherFactionClanTag, out var allianceRequest)
                || allianceRequest.IsRejected)
            {
                // no request or it's rejected
                return;
            }

            var otherFactionIncomingRequests =
                Faction.GetPrivateState(otherFaction).IncomingFactionAllianceRequests;

            officerFactionOutgoingRequests.Remove(otherFactionClanTag);
            otherFactionIncomingRequests.Remove(officerFactionClanTag);
            Logger.Important($"Alliance request with {otherFaction} cancel");
        }

        [RemoteCallSettings(timeInterval: 1, clientMaxSendQueueSize: 1)]
        private FactionDiplomacyRequestResult ServerRemote_OfficerProposeAlliance(string otherFactionClanTag)
        {
            SharedValidateAlliancesEnabled();

            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.DiplomacyManagement,
                                          out var officerFaction);

            var otherFaction = ServerGetFactionByClanTag(otherFactionClanTag,
                                                         throwExceptionIfNull: false);
            if (otherFaction is null)
            {
                return FactionDiplomacyRequestResult.ErrorUnknownClanTag;
            }

            if (otherFaction == officerFaction)
            {
                // should be impossible
                return FactionDiplomacyRequestResult.Success;
            }

            var currentRelationStatus = ServerGetFactionDiplomacyStatus(officerFaction, otherFaction);
            if (currentRelationStatus == FactionDiplomacyStatus.Ally)
            {
                // already have alliance
                return FactionDiplomacyRequestResult.Success;
            }

            Api.Assert(officerFaction != otherFaction, "Factions should be different");

            var officerFactionClanTag = SharedGetClanTag(officerFaction);
            var officerFactionIncomingRequests =
                Faction.GetPrivateState(officerFaction).IncomingFactionAllianceRequests;

            if (officerFactionIncomingRequests.TryGetValue(otherFactionClanTag, out var incomingRequest)
                && !incomingRequest.IsRejected)
            {
                // there is a valid incoming alliance request from the other faction,
                // simply accept it
                this.ServerRemote_OfficerSetDiplomacyStatus(otherFactionClanTag,
                                                            FactionDiplomacyStatusChangeRequest.Ally);
                return FactionDiplomacyRequestResult.Success;
            }

            var otherFactionIncomingRequests =
                Faction.GetPrivateState(otherFaction).IncomingFactionAllianceRequests;

            if (otherFactionIncomingRequests.TryGetValue(officerFactionClanTag, out _))
            {
                // there is already an alliance request
                // simply return success even if it's a rejected request
                // (this should not happen as the rejected requests are subject to cooldown
                // so client should not allow making new requests)
                return FactionDiplomacyRequestResult.Success;
            }

            var officerFactionOutgoingRequests =
                Faction.GetPrivateState(officerFaction).OutgoingFactionAllianceRequests;

            var time = Server.Game.FrameTime;
            var allianceRequest = new FactionAllianceRequest(
                expirationDate: time + FactionAllianceRequest.DefaultRequestLifetime,
                newRequestCooldownDate: time + FactionAllianceRequest.DefaultRequestCooldown);

            otherFactionIncomingRequests[officerFactionClanTag] = allianceRequest;
            officerFactionOutgoingRequests[otherFactionClanTag] = allianceRequest;

            Logger.Important(officerFaction + " submitted a faction alliance request to " + otherFaction);
            return FactionDiplomacyRequestResult.Success;
        }

        [RemoteCallSettings(timeInterval: 2)]
        private void ServerRemote_OfficerRejectAlliance(string otherFactionClanTag)
        {
            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.DiplomacyManagement,
                                          out var officerFaction);

            var officerFactionClanTag = SharedGetClanTag(officerFaction);
            var officerFactionIncomingRequests =
                Faction.GetPrivateState(officerFaction).IncomingFactionAllianceRequests;

            var otherFaction = ServerGetFactionByClanTag(otherFactionClanTag);
            if (otherFaction == officerFaction)
            {
                throw new InvalidOperationException();
            }

            if (!officerFactionIncomingRequests.TryGetValue(otherFactionClanTag, out var allianceRequest)
                || allianceRequest.IsRejected)
            {
                // no request or it's already rejected
                return;
            }

            var otherFactionOutgoingRequests =
                Faction.GetPrivateState(otherFaction).OutgoingFactionAllianceRequests;

            var rejectedApplication = allianceRequest.AsRejected(
                newRequestCooldownDate: Server.Game.FrameTime + FactionAllianceRequest.DefaultRequestCooldown);
            officerFactionIncomingRequests[otherFactionClanTag] = rejectedApplication;
            otherFactionOutgoingRequests[officerFactionClanTag] = rejectedApplication;
            Logger.Important($"Alliance with {otherFaction} rejected");
        }

        [RemoteCallSettings(timeInterval: 2)]
        private FactionDiplomacyRequestResult ServerRemote_OfficerSetDiplomacyStatus(
            string otherFactionClanTag,
            FactionDiplomacyStatusChangeRequest statusChangeRequest)
        {
            if (PveSystem.ServerIsPvE)
            {
                throw new Exception("There is no diplomacy management in PvE");
            }

            if (statusChangeRequest == FactionDiplomacyStatusChangeRequest.Ally)
            {
                SharedValidateAlliancesEnabled();
            }

            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.DiplomacyManagement,
                                          out var officerFaction);

            var otherFaction = ServerGetFactionByClanTag(otherFactionClanTag,
                                                         throwExceptionIfNull: false);
            if (otherFaction is null)
            {
                return FactionDiplomacyRequestResult.ErrorUnknownClanTag;
            }

            if (otherFaction == officerFaction)
            {
                // should be impossible
                return FactionDiplomacyRequestResult.ErrorUnknown;
            }

            var currentRelationStatus = ServerGetFactionDiplomacyStatus(officerFaction, otherFaction);
            if (!SharedIsDiplomacyStatusChangeRequired(currentRelationStatus, statusChangeRequest))
            {
                return FactionDiplomacyRequestResult.Success;
            }

            var otherFactionRelationStatus = ServerGetFactionDiplomacyStatus(otherFaction, officerFaction);

            switch (statusChangeRequest)
            {
                case FactionDiplomacyStatusChangeRequest.Neutral:
                    switch (currentRelationStatus)
                    {
                        case FactionDiplomacyStatus.Ally:
                        case FactionDiplomacyStatus.EnemyDeclaredByCurrentFaction:
                            ServerSetFactionDiplomacyStatusInternal(officerFaction,
                                                                    otherFaction,
                                                                    FactionDiplomacyStatus.Neutral,
                                                                    byOfficer: officer);
                            // other faction also goes neutral to the current faction
                            ServerSetFactionDiplomacyStatusInternal(otherFaction,
                                                                    officerFaction,
                                                                    FactionDiplomacyStatus.Neutral);
                            break;

                        case FactionDiplomacyStatus.EnemyMutual:
                            ServerSetFactionDiplomacyStatusInternal(officerFaction,
                                                                    otherFaction,
                                                                    FactionDiplomacyStatus.EnemyDeclaredByOtherFaction,
                                                                    byOfficer: officer);
                            // other faction remains enemy to the current faction
                            ServerSetFactionDiplomacyStatusInternal(otherFaction,
                                                                    officerFaction,
                                                                    FactionDiplomacyStatus
                                                                        .EnemyDeclaredByCurrentFaction);
                            break;

                        case FactionDiplomacyStatus.Neutral:
                        case FactionDiplomacyStatus.EnemyDeclaredByOtherFaction:
                            // no change
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;

                case FactionDiplomacyStatusChangeRequest.Enemy:
                    switch (otherFactionRelationStatus)
                    {
                        case FactionDiplomacyStatus.Neutral:
                        case FactionDiplomacyStatus.Ally:
                            // declare war
                            ServerSetFactionDiplomacyStatusInternal(officerFaction,
                                                                    otherFaction,
                                                                    FactionDiplomacyStatus
                                                                        .EnemyDeclaredByCurrentFaction,
                                                                    byOfficer: officer);

                            // other faction receives a war declaration from the current faction
                            ServerSetFactionDiplomacyStatusInternal(otherFaction,
                                                                    officerFaction,
                                                                    FactionDiplomacyStatus.EnemyDeclaredByOtherFaction);
                            break;

                        case FactionDiplomacyStatus.EnemyDeclaredByCurrentFaction:
                            // mutual war
                            ServerSetFactionDiplomacyStatusInternal(officerFaction,
                                                                    otherFaction,
                                                                    FactionDiplomacyStatus.EnemyMutual,
                                                                    byOfficer: officer);

                            ServerSetFactionDiplomacyStatusInternal(otherFaction,
                                                                    officerFaction,
                                                                    FactionDiplomacyStatus.EnemyMutual);
                            break;

                        case FactionDiplomacyStatus.EnemyMutual:
                            // no change
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;

                case FactionDiplomacyStatusChangeRequest.Ally:
                    // check whether there is a faction alliance request
                    var officerFactionIncomingRequests =
                        Faction.GetPrivateState(officerFaction).IncomingFactionAllianceRequests;
                    if (!officerFactionIncomingRequests.TryGetValue(SharedGetClanTag(otherFaction),
                                                                    out var allianceRequest)
                        || allianceRequest.IsRejected)
                    {
                        // not possible to create an alliance as there is no incoming request or it's rejected
                        break;
                    }

                    // there is a valid incoming alliance request from the other faction,
                    // accept it
                    var otherFactionOutgoingRequests =
                        Faction.GetPrivateState(otherFaction).OutgoingFactionAllianceRequests;
                    otherFactionOutgoingRequests.Remove(SharedGetClanTag(officerFaction));
                    officerFactionIncomingRequests.Remove(SharedGetClanTag(otherFaction));

                    ServerSetFactionDiplomacyStatusInternal(officerFaction,
                                                            otherFaction,
                                                            FactionDiplomacyStatus.Ally,
                                                            byOfficer: officer);

                    ServerSetFactionDiplomacyStatusInternal(otherFaction,
                                                            officerFaction,
                                                            FactionDiplomacyStatus.Ally);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(statusChangeRequest), statusChangeRequest, null);
            }

            return FactionDiplomacyRequestResult.Success;
        }

        public readonly struct FactionDiplomacyPublicStatusEntry : IRemoteCallParameter
        {
            public readonly string ClanTag;

            /// <summary>
            /// If true - alliance, if false - war.
            /// </summary>
            public readonly bool IsAlly;

            public FactionDiplomacyPublicStatusEntry(string clanTag, bool isAlly)
            {
                this.ClanTag = clanTag;
                this.IsAlly = isAlly;
            }
        }

        private class BootstrapperAllianceRequestsExpiration : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                TriggerEveryFrame.ServerRegister(ServerUpdateAllianceRequestsExpiration,
                                                 $"{nameof(FactionSystem)}.{nameof(ServerUpdateAllianceRequestsExpiration)}");
            }
        }
    }
}