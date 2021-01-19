namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryTerritoryAltered : BaseFactionEventLogEntry
    {
        public const string Name = "Faction territory altered";

        // {0} is a coordinate
        public const string Text_Expanded_Format
            = "Faction territory expanded at {0}.";

        // {0} is a coordinate
        public const string Text_Removed_Format
            = "Faction territory removed at {0}.";

        public FactionLogEntryTerritoryAltered(
            ICharacter byMember,
            Vector2Ushort worldPosition,
            bool isExpanded)
            : base(byMember)
        {
            this.WorldPosition = worldPosition;
            this.IsExpanded = isExpanded;
        }

        public override NotificationColor ClientNotificationColor
            => this.IsExpanded
                   ? NotificationColor.Good
                   : NotificationColor.Neutral;

        public override bool ClientShowNotification => true;

        public override string ClientText
            => string.Format(this.IsExpanded
                                 ? Text_Expanded_Format
                                 : Text_Removed_Format,
                             this.WorldPosition - Api.Client.World.WorldBounds.Offset);

        public bool IsExpanded { get; }

        public Vector2Ushort WorldPosition { get; }
    }
}