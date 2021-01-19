namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryFactionLevelUpgraded : BaseFactionEventLogEntry
    {
        public const string Name = "Faction level upgraded";

        public const string Text_Format
            = "Faction level upgraded to [b]{0}[/b].";

        public FactionLogEntryFactionLevelUpgraded(
            ICharacter byMember,
            byte toLevel)
            : base(byMember)
        {
            this.ToLevel = toLevel;
        }

        public override NotificationColor ClientNotificationColor => NotificationColor.Good;

        public override bool ClientShowNotification => true;

        public override string ClientText
            => string.Format(Text_Format, this.ToLevel);

        public byte ToLevel { get; }
    }
}