namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using JetBrains.Annotations;

    public partial class FactionSystem
    {
        /// <summary>
        /// Please do not modify this collection.
        /// </summary>
        public static readonly ObservableCollection<ClientApplicationEntry> ClientCurrentFactionReceivedApplications
            = IsClient
                  ? new ObservableCollection<ClientApplicationEntry>()
                  : null;

        /// <summary>
        /// Please do not modify this collection.
        /// </summary>
        public static readonly ObservableCollection<ClientApplicationEntry> ClientCurrentSubmittedApplications
            = IsClient
                  ? new ObservableCollection<ClientApplicationEntry>()
                  : null;

        [RemoteEnum]
        public enum ApplicationAcceptResult : byte
        {
            Unknown = 0,

            Success = 1,

            [Description(CoreStrings.Faction_FactionFull)]
            ErrorFactionFull = 2
        }

        [RemoteEnum]
        public enum ApplicationCreateResult : byte
        {
            Unknown = 0,

            Success = 1,

            [Description(CoreStrings.Faction_FactionFull)]
            ErrorFactionFull = 2,

            [Description(CoreStrings.Faction_NotAcceptingApplications)]
            NotAcceptingApplications = 3,

            [Description(CoreStrings.Faction_CannotRejoinInvitationRequired)]
            CannotRejoinInvitationRequired = 4
        }

        public static void ClientApplicantCancelApplication(string clanTag)
        {
            Instance.CallServer(
                _ => _.ServerRemote_ApplicantCancelApplication(clanTag));
        }

        public static void ClientApplicantSubmitApplication(string clanTag)
        {
            if (NewbieProtectionSystem.ClientIsNewbie)
            {
                NewbieProtectionSystem.ClientNotifyNewbieCannotPerformAction(null);
                return;
            }

            if (ClientCheckIsUnderJoinCooldown(showErrorNotification: true))
            {
                return;
            }

            DialogWindow.ShowDialog(
                title: CoreStrings.Faction_SubmitApplication,
                string.Format(CoreStrings.Faction_DialogSubmitApplication_Message_Format,
                              @$"\[{clanTag}\]")
                + "[br]"
                + "[br]"
                + CoreStrings.Faction_JoinCooldown_Description
                + " ("
                + ClientDefaultJoinCooldownDurationText
                + ")",
                okAction: async () =>
                          {
                              var result = await Instance.CallServer(
                                               _ => _.ServerRemote_ApplicantSubmitApplication(clanTag));

                              if (result != ApplicationCreateResult.Success)
                              {
                                  NotificationSystem.ClientShowNotification(
                                                        title: CoreStrings.Faction_Title,
                                                        result.GetDescription(),
                                                        NotificationColor.Bad)
                                                    .ViewModel
                                                    .Icon = ClientFactionEmblemCache.GetEmblemTextureBrush(clanTag);
                              }
                          },
                okText: CoreStrings.Faction_SubmitApplication,
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        public static async void ClientOfficerApplicationAccept(string applicantName)
        {
            ClientValidateHasAccessRights(FactionMemberAccessRights.Recruitment);
            var result = await Instance.CallServer(
                             _ => _.ServerRemote_OfficerApplicationAccept(applicantName));

            if (result != ApplicationAcceptResult.Success)
            {
                NotificationSystem.ClientShowNotification(
                    title: null,
                    result.GetDescription(),
                    NotificationColor.Bad);
            }
        }

        public static void ClientOfficerApplicationReject(string applicantName)
        {
            ClientValidateHasAccessRights(FactionMemberAccessRights.Recruitment);
            Instance.CallServer(
                _ => _.ServerRemote_OfficerApplicationReject(applicantName));
        }

        public static void ClientOfficerRejectAllApplications()
        {
            ClientValidateHasAccessRights(FactionMemberAccessRights.Recruitment);
            Instance.CallServer(
                _ => _.ServerRemote_OfficerRejectAllApplications());
        }

        public static void ClientOfficerSetIsAcceptingApplications(bool isAcceptingApplications)
        {
            ClientValidateHasAccessRights(FactionMemberAccessRights.Recruitment);
            Instance.CallServer(
                _ => _.ServerRemote_OfficerSetIsAcceptingApplications(isAcceptingApplications));
        }

        public static ApplicationAcceptResult ServerApplicationAccept(ICharacter officer, ICharacter applicant)
        {
            return ServerApplications.Accept(officer, applicant);
        }

        public static void ServerApplicationRemove(ILogicObject faction, ICharacter applicant)
        {
            ServerApplications.Remove(faction, applicant);
        }

        public static ApplicationAcceptResult ServerForceAcceptApplication(
            ICharacter applicant,
            ILogicObject faction)
        {
            return ServerApplications.AcceptWithoutValidation(officer: null, applicant, faction);
        }

        private void ClientRemote_ApplicantActiveApplicationsList(
            List<(string clanTag, double expirationDate)> applications)
        {
            foreach (var entry in ClientCurrentSubmittedApplications.ToList())
            {
                this.ClientRemote_ApplicantApplicationRemovedOrRejected(entry.ClanTag);
            }

            ClientCurrentSubmittedApplications.Clear();

            foreach (var application in applications)
            {
                this.ClientRemote_ApplicationAddedOrUpdated(application.clanTag, application.expirationDate);
            }
        }

        private void ClientRemote_ApplicantApplicationRemovedOrRejected(string clanTag)
        {
            for (var index = 0; index < ClientCurrentSubmittedApplications.Count; index++)
            {
                var application = ClientCurrentSubmittedApplications[index];
                if (!string.Equals(clanTag, application.ClanTag, StringComparison.Ordinal))
                {
                    continue;
                }

                ClientCurrentSubmittedApplications.RemoveAt(index);
                break;
            }
        }

        private void ClientRemote_ApplicationAddedOrUpdated(
            string clanTag,
            double expirationDate)
        {
            this.ClientRemote_ApplicantApplicationRemovedOrRejected(clanTag);

            var applicantName = Client.Characters.CurrentPlayerCharacter.Name;
            ClientCurrentSubmittedApplications.Add(
                new ClientApplicationEntry(clanTag,
                                           applicantName,
                                           expirationDate,
                                           default));
        }

        private void ClientRemote_ApplicationApprovedByOfficer(string clanTag, string officerName)
        {
            var notification = NotificationSystem.ClientShowNotification(
                CoreStrings.Faction_Title,
                string.Format(CoreStrings.Faction_NotificationCurrentPlayerApproved_Format,
                              officerName,
                              @$"\[{clanTag}\]"),
                NotificationColor.Good);
            notification.ViewModel.Icon = ClientFactionEmblemCache.GetEmblemTextureBrush(clanTag);
            notification.HideAfterDelay(60);
        }

        private void ClientRemote_OfficerFactionApplicationReceivedOrUpdated(
            string applicantName,
            double expirationDate,
            ushort learningPointsAccumulatedTotal)
        {
            this.ClientRemote_OfficerFactionApplicationRemoved(applicantName);

            ClientCurrentFactionReceivedApplications.Add(
                new ClientApplicationEntry(
                    SharedGetClanTag(ClientCurrentFaction),
                    applicantName,
                    expirationDate,
                    learningPointsAccumulatedTotal));
        }

        private void ClientRemote_OfficerFactionApplicationRemoved(string applicantName)
        {
            for (var index = 0; index < ClientCurrentFactionReceivedApplications.Count; index++)
            {
                var application = ClientCurrentFactionReceivedApplications[index];
                if (!string.Equals(applicantName, application.ApplicantName, StringComparison.Ordinal))
                {
                    continue;
                }

                ClientCurrentFactionReceivedApplications.RemoveAt(index);
                break;
            }
        }

        private void ClientRemote_OfficerFactionApplicationsList(
            List<(string applicantName, double expirationDate, ushort learningPointsAccumulatedTotal)> applications)
        {
            foreach (var entry in ClientCurrentFactionReceivedApplications.ToList())
            {
                this.ClientRemote_OfficerFactionApplicationRemoved(entry.ApplicantName);
            }

            ClientCurrentFactionReceivedApplications.Clear();

            foreach (var application in applications)
            {
                this.ClientRemote_OfficerFactionApplicationReceivedOrUpdated(
                    application.applicantName,
                    application.expirationDate,
                    application.learningPointsAccumulatedTotal);
            }
        }

        private void ServerRemote_ApplicantCancelApplication(string clanTag)
        {
            var applicant = ServerRemoteContext.Character;
            var faction = ServerGetFactionByClanTag(clanTag);
            ServerApplicationRemove(faction, applicant);
        }

        [RemoteCallSettings(timeInterval: 2)]
        private ApplicationCreateResult ServerRemote_ApplicantSubmitApplication(string clanTag)
        {
            var applicant = ServerRemoteContext.Character;
            var faction = ServerGetFactionByClanTag(clanTag);
            return ServerApplications.SubmitApplication(applicant, faction);
        }

        private ApplicationAcceptResult ServerRemote_OfficerApplicationAccept(string applicantName)
        {
            try
            {
                var officer = ServerRemoteContext.Character;
                ServerValidateHasAccessRights(officer, FactionMemberAccessRights.Recruitment);

                var applicant = Server.Characters.GetPlayerCharacter(applicantName);
                if (applicant is null)
                {
                    Logger.Warning("Inviter player not found: " + applicantName);
                    return ApplicationAcceptResult.Unknown;
                }

                return ServerApplications.Accept(officer, applicant);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return ApplicationAcceptResult.Unknown;
            }
        }

        private void ServerRemote_OfficerApplicationReject(string applicantName)
        {
            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.Recruitment,
                                          out var faction);

            var applicant = Server.Characters.GetPlayerCharacter(applicantName);
            if (applicant is null)
            {
                Logger.Warning("Inviter player not found: " + applicantName);
                return;
            }

            ServerApplications.Remove(faction, applicant);
        }

        private void ServerRemote_OfficerRejectAllApplications()
        {
            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.Recruitment,
                                          out var faction);

            ServerApplications.RemoveAllApplicationsInFaction(faction);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 2)]
        private void ServerRemote_OfficerSetIsAcceptingApplications(bool isAcceptingApplications)
        {
            var officer = ServerRemoteContext.Character;
            ServerValidateHasAccessRights(officer,
                                          FactionMemberAccessRights.Recruitment,
                                          out var faction);

            var publicState = Faction.GetPublicState(faction);
            if (publicState.Kind != FactionKind.Private)
            {
                throw new Exception("The faction is not private");
            }

            if (publicState.IsAcceptingApplications == isAcceptingApplications)
            {
                return;
            }

            publicState.IsAcceptingApplications = isAcceptingApplications;
            Logger.Important(
                string.Format(
                    "Changed faction \"is accepting applications\" flag for faction: {0}: {1}accepting applications",
                    faction,
                    isAcceptingApplications ? string.Empty : "not "));
        }

        public readonly struct ClientApplicationEntry
        {
            public readonly string ApplicantName;

            public readonly string ClanTag;

            public readonly double ExpirationDate;

            public readonly ushort LearningPointsAccumulatedTotal;

            public ClientApplicationEntry(
                string clanTag,
                string applicantName,
                double expirationDate,
                ushort learningPointsAccumulatedTotal)
            {
                this.ClanTag = clanTag;
                this.ApplicantName = applicantName;
                this.ExpirationDate = expirationDate;
                this.LearningPointsAccumulatedTotal = learningPointsAccumulatedTotal;
            }
        }

        private static class ServerApplications
        {
            public static ApplicationAcceptResult Accept(ICharacter officer, ICharacter applicant)
            {
                var faction = ServerGetFaction(officer);
                Api.Assert(faction is not null, "Officer must have a faction");
                ServerValidateHasAccessRights(officer, FactionMemberAccessRights.Recruitment);
                return AcceptWithoutValidation(officer, applicant, faction);
            }

            /// <summary>
            /// Don't use it unless for the debug commands.
            /// </summary>
            public static ApplicationAcceptResult AcceptWithoutValidation(
                [CanBeNull] ICharacter officer,
                ICharacter applicant,
                ILogicObject faction)
            {
                if (ServerGetFaction(applicant) is { } currentApplicantFaction)
                {
                    // applicant already has a faction
                    // maybe the Accept request was received more than once?
                    return ReferenceEquals(faction, currentApplicantFaction)
                               // already a member of the same faction
                               ? ApplicationAcceptResult.Success
                               : ApplicationAcceptResult.Unknown;
                }

                var application = GetApplication(faction, applicant);
                if (application is null)
                {
                    // should never happen
                    return ApplicationAcceptResult.Unknown;
                }

                var factionMembers = ServerGetFactionMembersReadOnly(faction);
                var maxMembers = FactionConstants.GetFactionMembersMax(
                    Faction.GetPublicState(faction).Kind);

                if (factionMembers.Count >= maxMembers)
                {
                    return ApplicationAcceptResult.ErrorFactionFull;
                }

                RemoveAllApplicationsByApplicant(applicant);
                ServerInvitations.RemoveAllInvitationsFor(applicant);

                ServerAddMember(applicant, faction, FactionMemberRole.Member);
                ServerAddLogEntry(faction,
                                  new FactionLogEntryMemberJoined(member: applicant,
                                                                  approvedByOfficer: officer));

                Logger.Important($"{applicant} joined {faction} - approved by faction officer {officer}");

                if (applicant.ServerIsOnline
                    && officer is not null)
                {
                    Instance.CallClient(
                        applicant,
                        _ => _.ClientRemote_ApplicationApprovedByOfficer(SharedGetClanTag(faction),
                                                                         officer.Name));
                }

                return ApplicationAcceptResult.Success;
            }

            public static List<FactionApplication> GetApplications(ILogicObject faction)
            {
                return Faction.GetPrivateState(faction).ServerApplicationsList;
            }

            public static void OfficerSyncApplicationsList(ICharacter officer, ILogicObject faction)
            {
                var applications = GetApplications(faction);
                if (applications.Count == 0)
                {
                    return;
                }

                // send a copy of the list
                var applicationsList = applications
                                       .Select(a => (a.Applicant.Name,
                                                     a.ExpirationDate,
                                                     GetLearningPointsAccumulatedTotal(a.Applicant)))
                                       .ToList();
                Instance.CallClient(officer,
                                    _ => _.ClientRemote_OfficerFactionApplicationsList(
                                        applicationsList));
            }

            public static void Remove(ILogicObject faction, ICharacter applicant)
            {
                Api.Assert(faction is not null, "No faction provided");
                var applications = GetApplications(faction);
                for (var index = 0; index < applications.Count; index++)
                {
                    var application = applications[index];
                    if (!ReferenceEquals(application.Applicant, applicant))
                    {
                        continue;
                    }

                    applications.RemoveAt(index);
                    NotifyApplicationRemoved(faction, applicant);
                    break;
                }

                // application not found
            }

            public static void RemoveAllApplicationsByApplicant(ICharacter applicant)
            {
                using var tempList = Api.Shared.GetTempList<ILogicObject>();
                ProtoFaction.GetAllGameObjects(tempList.AsList());

                foreach (var faction in tempList.AsList())
                {
                    var applications = GetApplications(faction);
                    for (var index = 0; index < applications.Count; index++)
                    {
                        var application = applications[index];
                        if (!ReferenceEquals(application.Applicant, applicant))
                        {
                            continue;
                        }

                        applications.RemoveAt(index);
                        index--;

                        NotifyApplicationRemoved(faction, applicant);
                    }
                }
            }

            public static void RemoveAllApplicationsInFaction(ILogicObject faction)
            {
                var applications = GetApplications(faction);
                try
                {
                    foreach (var application in applications)
                    {
                        NotifyApplicationRemoved(faction, application.Applicant);
                    }
                }
                finally
                {
                    applications.Clear();
                }
            }

            public static ApplicationCreateResult SubmitApplication(ICharacter applicant, ILogicObject faction)
            {
                if (ServerGetFaction(applicant) is not null)
                {
                    // already has a faction - should be impossible
                    return ApplicationCreateResult.Unknown;
                }

                if (NewbieProtectionSystem.SharedIsNewbie(applicant)
                    || SharedIsUnderJoinCooldown(applicant))
                {
                    // should be impossible as client performs the same check
                    return ApplicationCreateResult.Unknown;
                }

                if (Faction.GetPrivateState(faction).ServerPlayerLeaveDateDictionary
                           .ContainsKey(applicant.Id))
                {
                    return ApplicationCreateResult.CannotRejoinInvitationRequired;
                }

                if (GetApplication(faction, applicant)
                        is { } existingApplication)
                {
                    existingApplication.ResetExpirationDate();
                    NotifyApplicationReceivedOrUpdated(faction, applicant, existingApplication);

                    Logger.Info(
                        $"{applicant} has already submitted an application to {faction} - application timeout extended");
                    return ApplicationCreateResult.Success;
                }

                var factionPublicState = Faction.GetPublicState(faction);
                if (factionPublicState.Kind != FactionKind.Private)
                {
                    // should be impossible as client performs the same check
                    return ApplicationCreateResult.Unknown;
                }

                if (!factionPublicState.IsAcceptingApplications)
                {
                    return ApplicationCreateResult.NotAcceptingApplications;
                }

                var maxMembers = FactionConstants.GetFactionMembersMax(
                    Faction.GetPublicState(faction).Kind);

                if (ServerGetFactionMembersReadOnly(faction).Count >= maxMembers)
                {
                    return ApplicationCreateResult.ErrorFactionFull;
                }

                if (ServerInvitationAccept(invitee: applicant, faction)
                    == InvitationAcceptResult.Success)
                {
                    // there was already an invite to this faction and it was accepted now
                    // no need to submit an application
                    return ApplicationCreateResult.Success;
                }

                var application = new FactionApplication(applicant);
                GetApplications(faction)
                    .Add(application);

                Logger.Important($"{applicant} has submitted an application to {faction}");

                NotifyApplicationReceivedOrUpdated(faction, applicant, application);
                return ApplicationCreateResult.Success;
            }

            public static void SyncAllSubmittedApplicationsList(ICharacter applicant)
            {
                using var tempListApplications = Api.Shared.GetTempList<(string clanTag, double expirationDate)>();
                foreach (var faction in ServerGetFactionsTempList().EnumerateAndDispose())
                {
                    var applications = GetApplications(faction);
                    foreach (var application in applications)
                    {
                        if (!ReferenceEquals(application.Applicant, applicant))
                        {
                            continue;
                        }

                        tempListApplications.Add((SharedGetClanTag(faction),
                                                  application.ExpirationDate));
                        break;
                    }
                }

                if (tempListApplications.Count == 0)
                {
                    return;
                }

                // send a copy of the list
                var applicationsList = tempListApplications.AsList().ToList();
                Instance.CallClient(applicant,
                                    _ => _.ClientRemote_ApplicantActiveApplicationsList(
                                        applicationsList));
            }

            private static FactionApplication GetApplication(ILogicObject faction, ICharacter applicant)
            {
                foreach (var application in GetApplications(faction))
                {
                    if (ReferenceEquals(applicant, application.Applicant))
                    {
                        return application;
                    }
                }

                return null;
            }

            private static ushort GetLearningPointsAccumulatedTotal(ICharacter applicant)
            {
                return (ushort)Math.Min(applicant.SharedGetTechnologies().LearningPointsAccumulatedTotal,
                                        ushort.MaxValue);
            }

            private static void NotifyApplicationReceivedOrUpdated(
                ILogicObject faction,
                ICharacter applicant,
                FactionApplication application)
            {
                Instance.CallClient(applicant,
                                    _ => _.ClientRemote_ApplicationAddedOrUpdated(
                                        SharedGetClanTag(faction),
                                        application.ExpirationDate));

                Instance.CallClient(
                    ServerEnumerateFactionOfficers(faction, FactionMemberAccessRights.Recruitment),
                    _ => _.ClientRemote_OfficerFactionApplicationReceivedOrUpdated(applicant.Name,
                        application.ExpirationDate,
                        GetLearningPointsAccumulatedTotal(applicant)));
            }

            private static void NotifyApplicationRemoved(ILogicObject faction, ICharacter applicant)
            {
                Instance.CallClient(applicant,
                                    _ => _.ClientRemote_ApplicantApplicationRemovedOrRejected(
                                        SharedGetClanTag(faction)));

                Instance.CallClient(
                    ServerEnumerateFactionOfficers(faction, FactionMemberAccessRights.Recruitment),
                    _ => _.ClientRemote_OfficerFactionApplicationRemoved(applicant.Name));
            }

            private static void UpdateApplicationsExpiration()
            {
                // perform update once per minute per faction
                const double spreadDeltaTime = 60;
                var currentTime = Server.Game.FrameTime;

                using var tempListFactions = Api.Shared.GetTempList<ILogicObject>();

                ProtoFaction.EnumerateGameObjectsWithSpread(tempListFactions.AsList(),
                                                            spreadDeltaTime: spreadDeltaTime,
                                                            Server.Game.FrameNumber,
                                                            Server.Game.FrameRate);

                foreach (var faction in tempListFactions.AsList())
                {
                    var applications = GetApplications(faction);
                    for (var index = 0; index < applications.Count; index++)
                    {
                        var application = applications[index];
                        if (application.ExpirationDate > currentTime)
                        {
                            // Not expired.
                            // Please note we don't return here.
                            // It could be a nice optimization but not applicable here as
                            // the invitation expiration date could be extended.
                            continue;
                        }

                        // expired
                        applications.RemoveAt(index);
                        index--;

                        NotifyApplicationRemoved(faction, application.Applicant);
                    }
                }
            }

            private class Bootstrapper : BaseBootstrapper
            {
                public override void ServerInitialize(IServerConfiguration serverConfiguration)
                {
                    TriggerEveryFrame.ServerRegister(UpdateApplicationsExpiration,
                                                     $"{nameof(FactionSystem)}.{nameof(UpdateApplicationsExpiration)}");
                }
            }
        }

        [Serializable]
        public class FactionApplication
        {
            public readonly ICharacter Applicant;

            public FactionApplication(ICharacter applicant)
            {
                this.Applicant = applicant;
                this.ResetExpirationDate();
            }

            public double ExpirationDate { get; private set; }

            public void ResetExpirationDate()
            {
                this.ExpirationDate = Api.Server.Game.FrameTime
                                      + FactionConstants.FactionApplicationLifetimeSeconds;
            }
        }
    }
}