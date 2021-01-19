namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryRoleAccessRightsChanged : BaseFactionEventLogEntry
    {
        public const string CurrentPlayerAccessRightsChangedText
            = "Your faction access rights changed.";

        public const string Name = "Role access rights changed";

        public const string Text_Format
            = "Access rights for role [b]{0}[/b] changed.";

        public FactionLogEntryRoleAccessRightsChanged(
            ICharacter byOfficer,
            FactionOfficerRoleTitle roleTitle,
            FactionMemberAccessRights newAccessRights)
            : base(byOfficer)
        {
            this.RoleTitle = roleTitle;
            this.NewAccessRights = newAccessRights;
        }

        public override string ClientText
            => string.Format(Text_Format,
                             this.RoleTitle);

        public FactionMemberAccessRights NewAccessRights { get; }

        public FactionOfficerRoleTitle RoleTitle { get; }

        /// <summary>
        /// Show a custom notification for the affected player only.
        /// </summary>
        public override void ClientOnReceived()
        {
            var currentRole = FactionSystem.ClientCurrentRole;
            if (currentRole == FactionMemberRole.Leader
                || currentRole == FactionMemberRole.Member)
            {
                // not an officer
                return;
            }

            var currentOfficerRoleTitle = FactionSystem.ClientGetCurrentOfficerRoleTitle();
            if (!currentOfficerRoleTitle.HasValue
                || this.RoleTitle != currentOfficerRoleTitle.Value)
            {
                return;
            }

            var message = CurrentPlayerAccessRightsChangedText;
            if (!string.IsNullOrEmpty(this.ByMemberName))
            {
                message += "[br]"
                           + string.Format(Text_Name_Format, this.ByMemberName);
            }

            NotificationSystem.ClientShowNotification(
                CoreStrings.Faction_Title,
                message,
                NotificationColor.Good,
                icon: this.IconResource);
        }
    }
}