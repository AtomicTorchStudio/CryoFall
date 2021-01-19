namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Extensions;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryMemberRoleChanged : BaseFactionEventLogEntry
    {
        public const string CurrentPlayerRoleChangedText_Format
            = "Your faction role changed to [b]{0}[/b].";

        public const string Name = "Member role changed";

        public const string Text_Format
            = "[b]{0}[/b] role changed to [b]{1}[/b].";

        public FactionLogEntryMemberRoleChanged(
            ICharacter member,
            ICharacter byOfficer,
            FactionMemberRole newRole,
            FactionOfficerRoleTitle? newRoleTitle)
            : base(byOfficer)
        {
            this.MemberName = member.Name;
            this.NewRole = newRole;
            this.NewRoleTitle = newRoleTitle;
        }

        public string ClientRoleText
        {
            get
            {
                if (this.NewRoleTitle.HasValue)
                {
                    return this.NewRoleTitle.Value.GetDescription();
                }

                return this.NewRole switch
                {
                    FactionMemberRole.Member => CoreStrings.Faction_Role_Member,
                    FactionMemberRole.Leader => CoreStrings.Faction_Role_Leader,
                    _                        => "Unknown"
                };
            }
        }

        public override string ClientText
            => string.Format(Text_Format,
                             this.MemberName,
                             this.ClientRoleText);

        public string MemberName { get; }

        public FactionMemberRole NewRole { get; }

        public FactionOfficerRoleTitle? NewRoleTitle { get; }

        /// <summary>
        /// Show a custom notification for the affected player only.
        /// </summary>
        public override void ClientOnReceived()
        {
            if (this.MemberName != ClientCurrentCharacterHelper.Character.Name)
            {
                return;
            }

            var message = string.Format(CurrentPlayerRoleChangedText_Format,
                                        this.ClientRoleText);

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