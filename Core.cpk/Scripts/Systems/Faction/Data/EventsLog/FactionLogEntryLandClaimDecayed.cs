namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryLandClaimDecayed : BaseFactionEventLogEntry
    {
        public const string Name = "Faction land claim decayed";

        // {0} is a coordinate
        public const string Text_Format
            = "Faction land claim decayed at {0} and will be removed.";

        public FactionLogEntryLandClaimDecayed(
            Vector2Ushort worldPosition)
            : base(byMember: null)
        {
            this.WorldPosition = worldPosition;
        }

        public override NotificationColor ClientNotificationColor => NotificationColor.Bad;

        public override bool ClientShowNotification => true;

        public override string ClientText
            => string.Format(Text_Format, this.WorldPosition - Api.Client.World.WorldBounds.Offset);

        public Vector2Ushort WorldPosition { get; }
    }
}