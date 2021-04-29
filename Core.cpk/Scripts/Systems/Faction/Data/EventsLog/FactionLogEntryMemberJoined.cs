namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using JetBrains.Annotations;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryMemberJoined : BaseFactionEventLogEntry
    {
        public const string Name = "Member joined";

        public const string Text_Format
            = "[b]{0}[/b] joined the faction.";

        public FactionLogEntryMemberJoined(
            ICharacter member,
            [CanBeNull] ICharacter approvedByOfficer)
            : base(approvedByOfficer)
        {
            this.MemberName = member.Name;
        }

        public override bool ClientIsLongNotification => false;

        public override NotificationColor ClientNotificationColor => NotificationColor.Good;

        public override bool ClientShowNotification => true;

        public override string ClientText
            => string.Format(Text_Format,
                             this.MemberName);

        public string MemberName { get; }
    }
}