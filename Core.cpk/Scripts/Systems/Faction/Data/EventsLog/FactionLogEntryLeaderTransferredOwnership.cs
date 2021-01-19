namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryLeaderTransferredOwnership : BaseFactionEventLogEntry
    {
        public const string Name = "Leader transferred ownership";

        public const string Text_Format
            = "Leader transferred faction ownership to [b]{0}[/b].";

        public FactionLogEntryLeaderTransferredOwnership(
            ICharacter fromLeader,
            ICharacter toMember)
            : base(fromLeader)
        {
            this.ToMemberName = toMember.Name;
        }

        public override bool ClientIsLongNotification => true;

        public override bool ClientShowNotification => true;

        public override string ClientText
            => string.Format(Text_Format,
                             this.ToMemberName);

        public string ToMemberName { get; }
    }
}