namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    [Description(Name)]
    [Serializable]
    public class FactionLogEntryMemberLeft : BaseFactionEventLogEntry
    {
        public const string Name = "Member left";

        public const string Text_Format
            = "[b]{0}[/b] left the faction.";

        public FactionLogEntryMemberLeft(ICharacter member)
            : base(byMember: null)
        {
            this.MemberName = member.Name;
        }

        public override string ClientText
            => string.Format(Text_Format,
                             this.MemberName);

        public string MemberName { get; }
    }
}